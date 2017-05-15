using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyBF.Parsing;

namespace CyBF.BFIL
{
    public class BFILLoopStatement : BFILStatement
    {
        public IReadOnlyList<BFILStatement> Body { get; private set; }

        public BFILLoopStatement(Token reference, IEnumerable<BFILStatement> body) 
            : base(reference)
        {
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress)
        {
            int startingAddress = currentAddress;

            bfoutput.Append("[");

            foreach (BFILStatement statement in this.Body)
                statement.Compile(bfoutput, variables, ref currentAddress);

            bfoutput.Append("]");

            int endingAddress = currentAddress;

            if (startingAddress != endingAddress)
                throw new BFILProgramError(this.ReferenceToken, "Loop does not end on starting variable.");
        }

        public override void PrintDebugSource(StringBuilder output, ReferenceTable variables, int indent)
        {
            output.AppendLine(new string('\t', indent) + "[");

            foreach (BFILStatement statement in this.Body)
                statement.PrintDebugSource(output, variables, indent + 1);

            output.AppendLine(new string('\t', indent) + "]");
        }
    }
}
