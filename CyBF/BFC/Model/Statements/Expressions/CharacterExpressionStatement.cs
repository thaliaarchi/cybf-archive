using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using System.Text;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class CharacterExpressionStatement : ExpressionStatement
    {
        public string LiteralString { get; private set; }
        public char Character { get; private set; }
        public byte Ordinal { get; private set; }
        
        public CharacterExpressionStatement(Token reference) 
            : base(reference)
        {
            this.LiteralString = reference.TokenString;
            this.Character = Encoding.ASCII.GetChars(reference.AsciiBytes)[0];
            this.Ordinal = (byte)reference.NumericValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = new BFObject(new CharacterInstance(this.LiteralString, this.Character, this.Ordinal));
        }

        public override bool IsVolatile()
        {
            return false;
        }
    }
}
