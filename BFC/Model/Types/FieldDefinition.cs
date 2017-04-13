namespace CyBF.BFC.Model.Types
{
    public class FieldDefinition
    {
        public string Name { get; private set; }
        public TypeVariable DataType { get; private set; }

        public FieldDefinition(string name, TypeVariable dataType)
        {
            this.Name = name;
            this.DataType = dataType;
        }

        public FieldInstance MakeInstance(int offset)
        {
            return new FieldInstance(this.Name, this.DataType.Value, offset);
        }
    }
}
