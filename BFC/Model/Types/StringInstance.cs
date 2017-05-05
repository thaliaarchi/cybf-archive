namespace CyBF.BFC.Model.Types
{
    public class StringInstance : TypeInstance
    {
        public string RawString { get; private set; }
        public string ProcessedString { get; private set; }

        public StringInstance(string rawString, string processedString) 
            : base(
                  StringDefinition.StaticName,
                  new TypeInstance[] { },
                  new FieldInstance[] {
                      new FieldInstance("length", new ConstInstance(processedString.Length), 0),
                      new FieldInstance("size", new ConstInstance(processedString.Length + 2), 0)})
        {
            this.RawString = rawString;
            this.ProcessedString = processedString;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
