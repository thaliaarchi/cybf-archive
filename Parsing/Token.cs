
namespace CyBF.Parsing
{
    public class Token
    {
        public PositionInfo PositionInfo { get; set; }
        public TokenType TokenType { get; set; }
        public string Value { get; set; }
        public int NumericValue { get; set; }

        public Token(PositionInfo positionInfo, TokenType tokenType, string value, int numericValue)
        {
            this.PositionInfo = positionInfo;
            this.TokenType = tokenType;
            this.Value = value;
            this.NumericValue = numericValue;
        }

        public override string ToString()
        {
            return this.TokenType.ToString() + ":" + this.Value;
        }
    }
}
