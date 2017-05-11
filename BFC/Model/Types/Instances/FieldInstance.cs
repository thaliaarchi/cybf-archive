namespace CyBF.BFC.Model.Types.Instances
{
    public class FieldInstance
    {
        public string Name { get; private set; }
        public TypeInstance DataType { get; private set; }
        public int Offset { get; private set; }

        public FieldInstance(string name, TypeInstance dataType, int offset)
        {
            this.Name = name;
            this.DataType = dataType;
            this.Offset = offset;
        }
    }
}
