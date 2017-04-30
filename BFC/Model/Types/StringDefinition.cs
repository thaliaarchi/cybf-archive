using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;

namespace CyBF.BFC.Model.Types
{
    public class StringDefinition : TypeDefinition
    {
        public const string StaticName = "String";

        public StringDefinition()
            : base(
                  new TypeConstraint(StaticName),
                  new Variable("id"))
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);

            int stringId = ((ConstInstance)valueArguments.Single().DataType).Value;

            if (!compiler.ContainsCachedString(stringId))
                compiler.RaiseSemanticError("Invalid String ID.");

            CyBFString value = compiler.GetCachedString(stringId);

            return new StringInstance(value);
        }
    }
}
