using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Statements;
using CyBF.Parsing;
using System.Linq;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class StructDefinition : TypeDefinition
    {
        public Token Reference { get; private set; }
        public IReadOnlyList<FieldDefinition> Fields { get; private set; }

        public StructDefinition(
            Token reference, 
            TypeConstraint constraint, 
            IEnumerable<Variable> parameters,
            IEnumerable<FieldDefinition> fields,
            IEnumerable<FunctionDefinition> methods)
            : base(constraint, parameters, methods)
        {
            this.Reference = reference;
            
            List<FieldDefinition> fieldsList = new List<FieldDefinition>();

            foreach (FieldDefinition field in fields)
            {
                if (fieldsList.Any(f => f.Name == field.Name))
                    throw new SemanticError("Duplicate field definition.", field.Reference, this.Reference);

                fieldsList.Add(field);
            }

            this.Fields = fieldsList.AsReadOnly();
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            compiler.TracePush(this.Reference);
            
            this.ApplyArguments(compiler, typeArguments, valueArguments);

            List<FieldInstance> fieldInstances = new List<FieldInstance>();
            int offset = 0;

            using (compiler.BeginRecursionCheck(this))
            {
                foreach (FieldDefinition fieldDefinition in this.Fields)
                {
                    FieldInstance fieldInstance = fieldDefinition.Compile(compiler, offset);
                    offset += fieldInstance.DataType.Size();
                    fieldInstances.Add(fieldInstance);
                }
            }

            StructInstance structInstance = new StructInstance(this.Reference, this.Name, typeArguments, fieldInstances, this.Methods);

            compiler.TracePop();

            return structInstance;
        }
    }
}
