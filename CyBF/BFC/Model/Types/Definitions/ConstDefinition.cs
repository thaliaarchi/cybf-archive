using System.Collections.Generic;
using CyBF.BFC.Compilation;
using System.Linq;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class ConstDefinition : TypeDefinition
    {
        public const string StaticName = "Const";

        public ConstDefinition()
            : base(
                new TypeConstraint(StaticName),
                new SystemVariable())
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
