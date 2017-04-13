using System.Collections.Generic;
using CyBF.BFC.Compilation;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public class ConstDefinition : TypeDefinition
    {
        public const string StaticName = "Const";

        public ConstDefinition()
            : base(
                new TypeConstraint(StaticName, new TypeParameter[] { }),
                new Variable[] { new Variable() })
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            int value = ((ConstInstance)valueArguments.Single().DataType).Value;
            return new ConstInstance(value);
        }
    }
}
