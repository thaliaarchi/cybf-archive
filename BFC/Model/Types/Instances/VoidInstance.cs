using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class VoidInstance : TypeInstance
    {
        public VoidInstance() 
            : base(VoidDefinition.StaticName)
        {
        }

        public override int Size()
        {
            return 0;
        }
    }
}
