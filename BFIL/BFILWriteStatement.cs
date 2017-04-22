using System.Collections.Generic;
using System.Linq;
using CyBF.Parsing;

namespace CyBF.BFIL
{
    public class BFILWriteStatement : BFILStatement
    {
        public IReadOnlyList<byte> Data { get; private set; }

        public BFILWriteStatement(Token reference, IEnumerable<byte> data)
            : base(reference)
        {
            this.Data = data.ToList().AsReadOnly();
        }

        public override void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress)
        {
            foreach (byte b in this.Data)
            {
                bfoutput.Append("[-]");
                bfoutput.Append(new string('+', b));
                bfoutput.Append(">");
            }

            bfoutput.Append(new string('<', this.Data.Count));
        }
    }
}
