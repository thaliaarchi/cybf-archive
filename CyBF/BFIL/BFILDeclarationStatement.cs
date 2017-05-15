using CyBF.Parsing;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace CyBF.BFIL
{
    public class BFILDeclarationStatement : BFILStatement
    {
        public string Name { get; private set; }
        public int Size { get; private set; }
        public IReadOnlyList<byte> Data { get; private set; }

        private BFILReferenceStatement _variableReference;
        private BFILWriteStatement _variableWrite;

        public BFILDeclarationStatement(Token reference, string name, int size)
            : base(reference)
        {
            this.Name = name;
            this.Data = new List<byte>(0).AsReadOnly();
            this.Size = size;

            _variableReference = new BFILReferenceStatement(reference, this.Name);
            _variableWrite = new BFILWriteStatement(reference, this.Data);
        }

        public BFILDeclarationStatement(Token reference, string name, IEnumerable<byte> data)
            : base(reference)
        {
            this.Name = name;
            this.Data = data.ToList().AsReadOnly();
            this.Size = this.Data.Count;

            _variableReference = new BFILReferenceStatement(reference, this.Name);
            _variableWrite = new BFILWriteStatement(reference, this.Data);
        }

        public override void Compile(BFStringBuilder bfoutput, ReferenceTable variables, ref int currentAddress)
        {
            _variableReference.Compile(bfoutput, variables, ref currentAddress);
            _variableWrite.Compile(bfoutput, variables, ref currentAddress);
        }

        public override void PrintDebugSource(StringBuilder output, ReferenceTable variables, int indent)
        {
            output.Append(new string('\t', indent));

            string debugName = this.Name;

            if (variables.Contains(this.Name))
                debugName = variables[this.Name].DebugName;

            if (this.Data.Count == 0)
                output.AppendLine("@" + debugName + ":" + this.Size.ToString());
            else
                output.AppendLine("@" + debugName + "#(" + string.Join(", ", this.Data) + ")");
        }
    }
}
