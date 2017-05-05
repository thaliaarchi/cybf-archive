namespace CyBF.BFC.Model.Types
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
