namespace CyBF.BFC.Model.Types
{
    public class StringInstance : TypeInstance
    {
        public CyBFString String { get; private set; }

        public StringInstance(CyBFString value) 
            : base(StringDefinition.StaticName)
        {
            this.String = value;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
