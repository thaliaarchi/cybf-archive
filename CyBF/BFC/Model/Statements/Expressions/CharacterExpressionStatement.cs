using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class CharacterExpressionStatement : ExpressionStatement
    {
        public string RawString { get; private set; }
        public string ProcessedString { get; private set; }
        public int Ordinal { get; private set; }

        public CharacterExpressionStatement(Token reference, string rawString, string processedString, int ordinal) 
            : base(reference)
        {
            this.RawString = rawString;
            this.ProcessedString = processedString;
            this.Ordinal = ordinal;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = new BFObject(new CharacterInstance(this.RawString, this.ProcessedString, this.Ordinal));
        }

        public override bool IsVolatile()
        {
            return false;
        }
    }
}
