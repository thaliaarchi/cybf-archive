using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Statements;
using CyBF.Parsing;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public class StructDefinition : TypeDefinition
    {
        public Token Reference { get; private set; }
        public IReadOnlyList<Statement> SetupStatements { get; private set; }
        public IReadOnlyList<FieldDefinition> Fields { get; private set; }

        public StructDefinition(
            Token reference, 
            TypeConstraint constraint, 
            IEnumerable<Variable> parameters,
            IEnumerable<Statement> setupStatements,
            IEnumerable<FieldDefinition> fields)
            : base(constraint, parameters)
        {
            this.Reference = reference;
            this.SetupStatements = setupStatements.ToList().AsReadOnly();
            this.Fields = fields.ToList().AsReadOnly();
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            compiler.TracePush(this.Reference);

            this.ApplyArguments(compiler, typeArguments, valueArguments);

            foreach (Statement statement in this.SetupStatements)
                statement.Compile(compiler);

            int offset = 0;
            List<FieldInstance> instanceFields = new List<FieldInstance>();

            foreach (FieldDefinition defField in this.Fields)
            {
                FieldInstance instField = defField.MakeInstance(offset);
                offset += instField.DataType.Size();
                instanceFields.Add(instField);
            }

            compiler.TracePop();

            return new StructInstance(this.Reference, this.TypeName, typeArguments, instanceFields);
        }
    }
}
