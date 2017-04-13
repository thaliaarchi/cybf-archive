namespace CyBF.BFC.Model.Types
{
    public class ByteInstance : TypeInstance
    {
        public ByteInstance() 
            : base(ByteDefinition.StaticName, new TypeInstance[] { }, new FieldInstance[] { })
        {
        }

        public override int Size()
        {
            return 1;
        }
    }
}
