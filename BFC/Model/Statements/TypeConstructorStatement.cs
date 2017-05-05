using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class TypeConstructorStatement : TypeExpressionStatement
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeExpressionStatement> TypeArguments { get; private set; }
        public IReadOnlyList<ExpressionStatement> ValueArguments { get; private set; }
        
        public TypeConstructorStatement(
            Token reference, 
            string typeName, 
            IEnumerable<TypeExpressionStatement> typeArguments, 
            IEnumerable<ExpressionStatement> valueArguments)
            : base(reference)
        {
            this.TypeName = typeName;
            this.TypeArguments = typeArguments.ToList().AsReadOnly();
            this.ValueArguments = valueArguments.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            foreach (TypeExpressionStatement typeArgument in this.TypeArguments)
                typeArgument.Compile(compiler);

            foreach (ExpressionStatement valueArgument in this.ValueArguments)
                valueArgument.Compile(compiler);

            IEnumerable<TypeInstance> typeArgumentInstances = this.TypeArguments.Select(arg => arg.ReturnVariable.Value);
            IEnumerable<BFObject> valueArgumentObjects = this.ValueArguments.Select(arg => arg.ReturnVariable.Value);

            compiler.TracePush(this.Reference);

            List<TypeDefinition> matches = compiler.MatchType(this.TypeName, typeArgumentInstances);

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
            this.ReturnVariable.Value = definition.Compile(compiler, typeArgumentInstances, valueArgumentObjects);

            compiler.TracePop();
        }
    }
}
