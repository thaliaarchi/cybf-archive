using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class ByteDefinition : TypeDefinition
    {
        public const string StaticName = "Byte";

        public ByteDefinition()
            : base(new TypeConstraint(StaticName))
        {
        }

        protected override TypeInstance OnCompile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            return new ByteInstance();
        }
    }
}
