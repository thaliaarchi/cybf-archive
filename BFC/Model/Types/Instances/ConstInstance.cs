using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class ConstInstance : TypeInstance
    {
        public int Value { get; private set; }

        public ConstInstance(int value)
            : base(ConstDefinition.StaticName)
        {
            this.Value = value;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
