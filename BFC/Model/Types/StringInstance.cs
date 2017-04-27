namespace CyBF.BFC.Model.Types
{
    public class StringInstance : TypeInstance
    {
        public CyBFString String { get; private set; }

        public StringInstance(CyBFString value) 
            : base(StringDefinition.StaticName, new TypeInstance[] { }, new FieldInstance[] { })
        {
            this.String = value;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
