using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class ByteInstance : TypeInstance
    {
        public ByteInstance() 
            : base(ByteDefinition.StaticName)
        {
        }

        public override int Size()
        {
            return 1;
        }
    }
}
