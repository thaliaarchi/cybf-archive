namespace CyBF.BFC.Model.Data
{
    public abstract class Variable
    {
        public string Name { get; protected set; }
        public BFObject Value { get; set; }
    }
}
