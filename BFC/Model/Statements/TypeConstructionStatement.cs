using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class TypeConstructionStatement : Statement
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeVariable> TypeArguments { get; private set; }
        public IReadOnlyList<Variable> ValueArguments { get; private set; }
        public TypeVariable ReturnValue { get; private set; }

        public TypeConstructionStatement(
            Token reference, 
            string typeName, 
            IEnumerable<TypeVariable> typeArguments, 
            IEnumerable<Variable> valueArguments,
            TypeVariable returnValue) 
            : base(reference)
        {
            this.TypeName = typeName;
            this.TypeArguments = typeArguments.ToList().AsReadOnly();
            this.ValueArguments = valueArguments.ToList().AsReadOnly();
            this.ReturnValue = returnValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.TracePush(this.Reference);

            List<TypeDefinition> matches = compiler.MatchType(this.TypeName, this.TypeArguments.Select(a => a.Value));

            if (matches.Count == 0)
                compiler.RaiseSemanticError("No matching type definitions found.");

            if (matches.Count > 1)
            {
                IEnumerable<StructDefinition> structs = matches
                    .Where(m => m is StructDefinition)
                    .Select(m => m as StructDefinition);

                List<Token> references = new List<Token>();

                references.Add(this.Reference);

                foreach (StructDefinition str in structs)
                    references.Add(str.Reference);

                throw new SemanticError("Ambiguous type constructor.", references);
            }

            TypeDefinition definition = matches.Single();

            IEnumerable<TypeInstance> typeArgInstances = this.TypeArguments.Select(v => v.Value);
            IEnumerable<BFObject> valArgInstances = this.ValueArguments.Select(v => v.Value);
            TypeInstance instance = definition.Compile(compiler, typeArgInstances, valArgInstances);

            this.ReturnValue.Value = instance;

            compiler.TracePop();
        }
    }
}
