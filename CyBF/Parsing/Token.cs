
namespace CyBF.Parsing
{
    public class Token
    {
        public PositionInfo PositionInfo { get; set; }
        public TokenType TokenType { get; set; }
        public string TokenString { get; set; }

        public int NumericValue { get; set; }
        public byte[] AsciiBytes { get; set; }

        public Token(PositionInfo positionInfo, TokenType tokenType, string tokenString)
        {
            this.PositionInfo = positionInfo;
            this.TokenType = tokenType;
            this.TokenString = tokenString;

            this.NumericValue = 0;
            this.AsciiBytes = null;
        }

        public override string ToString()
        {
            return this.TokenType.ToString() + ":" + this.TokenString;
        }
    }
}
