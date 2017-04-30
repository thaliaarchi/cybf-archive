namespace CyBF.BFC.Model
{
    public class Variable
    {
        private static int _variableAutonum = 1;

        public string Name { get; private set; }
        public BFObject Value { get; set; }

        public Variable()
        {
            this.Name = "var" + (_variableAutonum++).ToString();
        }

        public Variable(string name)
        {
            this.Name = name;
        }
    }
}
