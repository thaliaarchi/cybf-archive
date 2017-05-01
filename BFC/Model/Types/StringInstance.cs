namespace CyBF.BFC.Model.Types
{
    public class StringInstance : TypeInstance
    {
        public CyBFString String { get; private set; }

        public StringInstance(CyBFString value) 
            : base(
                  StringDefinition.StaticName,
                  new TypeInstance[] { },
                  new FieldInstance[] {
                      new FieldInstance("length", new ConstInstance(value.ProcessedValue.Length), 0),
                      new FieldInstance("size", new ConstInstance(value.ProcessedValue.Length + 2), 0)})
        {
            this.String = value;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
