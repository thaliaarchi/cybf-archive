namespace CyBF.BFC.Model.Types
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
