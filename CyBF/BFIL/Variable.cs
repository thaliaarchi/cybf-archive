namespace CyBF.BFIL
{
    public class Variable
    {
        public string Name { get; private set; }
        public int Size { get; private set; }

        public int Address { get; set; }

        public string DebugName
        {
            get
            {
                if (this.Address >= 0)
                    return string.Format("{0}_sz{1}_adr{2}", this.Name, this.Size, this.Address);
                else
                    return string.Format("{0}_sz{1}_adrNULL", this.Name, this.Size, this.Address);
            }
        }

        public Variable(string name, int size)
        {
            this.Name = name;
            this.Size = size;
            this.Address = -1;
        }
    }
}
