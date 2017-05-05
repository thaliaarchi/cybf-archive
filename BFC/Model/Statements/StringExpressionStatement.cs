using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class StringExpressionStatement : ExpressionStatement
    {
        public string RawString { get; private set; }
        public string ProcessedString { get; private set; }

        public StringExpressionStatement(Token reference, string rawString, string processedString) 
            : base(reference)
        {
            this.RawString = rawString;
            this.ProcessedString = processedString;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = new BFObject(new StringInstance(this.RawString, this.ProcessedString));
        }
    }
}
