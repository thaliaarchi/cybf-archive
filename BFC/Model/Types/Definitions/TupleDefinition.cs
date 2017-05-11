using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class TupleDefinition : TypeDefinition
    {
        public const string StaticName = "Tuple";

        public TupleDefinition() 
            : base(new TypeConstraint(StaticName))
                
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            return new TupleInstance(new BFObject[] { });
        }
    }
}
