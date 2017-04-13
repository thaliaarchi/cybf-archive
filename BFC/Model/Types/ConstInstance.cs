namespace CyBF.BFC.Model.Types
{
    public class ConstInstance : TypeInstance
    {
        public int Value { get; private set; }

        public ConstInstance(int value)
            : base(ConstDefinition.StaticName, new TypeInstance[] { }, new FieldInstance[] { })
        {
            this.Value = value;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
