using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Types
{
    public class ByteDefinition : TypeDefinition
    {
        public const string StaticName = "Byte";

        public ByteDefinition()
            : base(new TypeConstraint(StaticName))
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            return new ByteInstance();
        }
    }
}
