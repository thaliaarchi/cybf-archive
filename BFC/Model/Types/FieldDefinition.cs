using CyBF.Parsing;

namespace CyBF.BFC.Model.Types
{
    public class FieldDefinition
    {
        public Token Reference { get; private set; }
        public string Name { get; private set; }
        public TypeVariable DataType { get; private set; }

        public FieldDefinition(Token reference, string name, TypeVariable dataType)
        {
            this.Reference = reference;
            this.Name = name;
            this.DataType = dataType;
        }

        public FieldInstance MakeInstance(int offset)
        {
            return new FieldInstance(this.Name, this.DataType.Value, offset);
        }
    }
}
