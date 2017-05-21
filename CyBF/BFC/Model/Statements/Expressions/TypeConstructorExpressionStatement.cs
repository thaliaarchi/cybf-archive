using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Statements.Expressions
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

            TypeDefinition definition = compiler.ResolveType(this.TypeName, typeArgumentInstances);
            this.ReturnVariable.Value = definition.Compile(compiler, typeArgumentInstances, valueArgumentObjects);

            compiler.TracePop();
        }
    }
}
