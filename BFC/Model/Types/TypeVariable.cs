namespace CyBF.BFC.Model.Types
{
    public class TypeVariable
    {
        private static int _typeVariableAutonum = 1;

        public string Name { get; private set; }
        public TypeInstance Value { get; set; }

        public TypeVariable()
        {
            this.Name = "~typ" + (_typeVariableAutonum++).ToString();
        }

        public TypeVariable(string name)
        {
            this.Name = name;
        }
    }
}
