using CyBF.Parsing;

namespace CyBF.BFIL
{
    public abstract class BFILStatement
    {
        public Token ReferenceToken { get; private set; }

        public BFILStatement(Token referenceToken)
        {
            this.ReferenceToken = referenceToken;
        }

        public abstract void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress);
    }
}
