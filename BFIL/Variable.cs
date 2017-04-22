namespace CyBF.BFIL
{
    public class Variable
    {
        public string Name { get; private set; }
        public int Size { get; private set; }

        public int Address { get; set; }

        public Variable(string name, int size)
        {
            this.Name = name;
            this.Size = size;
            this.Address = -1;
        }
    }
}
