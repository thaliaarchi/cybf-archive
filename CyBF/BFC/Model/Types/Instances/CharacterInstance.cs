using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class CharacterInstance : TypeInstance
    {
        public string RawString { get; private set; }
        public string ProcessedString { get; private set; }
        public int Ordinal { get; private set; }

        public CharacterInstance(string rawString, string processedString, int ordinal)
            : base(
                  CharacterDefinition.StaticName,
                  new TypeInstance[] { },
                  new FieldInstance[] {
                      new FieldInstance("ordinal", new ConstInstance(ordinal), 0) })
        {
            this.RawString = rawString;
            this.ProcessedString = processedString;
            this.Ordinal = ordinal;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
