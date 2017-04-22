using CyBF.Parsing;

namespace CyBF.BFIL
{
    public class BFILDeclarationStatement : BFILStatement
    {
        public string Name { get; private set; }
        public int Size { get; private set; }

        public BFILReferenceStatement VariableReference { get; private set; }

        public BFILDeclarationStatement(Token reference, string name, int size) 
            : base(reference)
        {
            this.Name = name;
            this.Size = size;

            this.VariableReference = new BFILReferenceStatement(this.ReferenceToken, this.Name);
        }

        public override void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress)
        {
            this.VariableReference.Compile(bfoutput, variables, ref currentAddress);
        }
    }
}
