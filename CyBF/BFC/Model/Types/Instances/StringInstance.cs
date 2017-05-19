using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class StringInstance : TypeInstance
    {
        public string LiteralString { get; private set; }
        public string ProcessedString { get; private set; }
        public byte[] AsciiBytes { get; private set; }

        public StringInstance(string literalString, string processedString, byte[] asciiBytes) 
            : base(
                  StringDefinition.StaticName,
                  new TypeInstance[] { },
                  new FieldInstance[] {
                      new FieldInstance("length", new ConstInstance(processedString.Length), 0),
                      new FieldInstance("size", new ConstInstance(processedString.Length + 2), 0)})
        {
            this.LiteralString = literalString;
            this.ProcessedString = processedString;
            this.AsciiBytes = asciiBytes;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
