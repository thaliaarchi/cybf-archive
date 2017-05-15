using System;
using System.Text;
using CyBF.Parsing;

namespace CyBF.BFIL
{
    public class BFILReferenceStatement : BFILStatement
    {
        public string Name { get; private set; }

        public BFILReferenceStatement(Token reference, string name) 
            : base(reference)
        {
            this.Name = name;
        }

        public override void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress)
        {
            Variable variable = variables[this.Name];

            if (variable.Address < currentAddress)
                bfoutput.Append(new string('<', currentAddress - variable.Address));
            else
                bfoutput.Append(new string('>', variable.Address - currentAddress));

            currentAddress = variable.Address;
        }

        public override void PrintDebugSource(StringBuilder output, ReferenceTable variables, int indent)
        {
            output.Append(new string('\t', indent));

            string debugName = this.Name;

            if (variables.Contains(this.Name))
                debugName = variables[this.Name].DebugName;

            output.AppendLine(debugName);
        }
    }
}
