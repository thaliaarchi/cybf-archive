using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class CharacterInstance : TypeInstance
    {
        public string LiteralString { get; private set; }
        public char Character { get; private set; }
        public byte Ordinal { get; private set; }

        public CharacterInstance(string literalString, char character, byte ordinal)
            : base(
                  CharacterDefinition.StaticName,
                  new TypeInstance[] { },
                  new FieldInstance[] {
                      new FieldInstance("ordinal", new ConstInstance(ordinal), 0) })
        {
            this.LiteralString = literalString;
            this.Character = character;
            this.Ordinal = ordinal;
        }

        public override int Size()
        {
            return 0;
        }
    }
}
