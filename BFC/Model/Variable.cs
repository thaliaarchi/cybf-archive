namespace CyBF.BFC.Model
{
    public class Variable
    {
        public string Name { get; private set; }
        public BFObject Value { get; set; }

        public Variable(string name)
        {
            this.Name = name;
        }
    }
}
