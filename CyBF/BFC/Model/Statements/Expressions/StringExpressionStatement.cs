using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;
using System;
using CyBF.BFC.Model.Types.Instances;
using System.Text;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class StringExpressionStatement : ExpressionStatement
    {
        public string LiteralString { get; private set; }
        public string ProcessedString { get; private set; }
        public byte[] AsciiBytes { get; private set; }

        public StringExpressionStatement(Token reference) 
            : base(reference)
        {
            this.LiteralString = reference.TokenString;
            this.ProcessedString = Encoding.ASCII.GetString(reference.AsciiBytes);
            this.AsciiBytes = reference.AsciiBytes;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = new BFObject(
                new StringInstance(this.LiteralString, this.ProcessedString, this.AsciiBytes));
        }

        public override bool IsVolatile()
        {
            return false;
        }
    }
}
