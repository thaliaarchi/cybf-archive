using CyBF.Parsing;

namespace CyBF.BFIL
{
    public class BFILCommandStatement : BFILStatement
    {
        public string Commands { get; private set; }

        public BFILCommandStatement(Token reference, string commands) 
            : base(reference)
        {
            this.Commands = commands;
        }

        public override void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress)
        {
            bfoutput.Append(this.Commands);
        }
    }
}
