
namespace CyBF.Parsing
{
    public class Token
    {
        public PositionInfo PositionInfo { get; set; }
        public TokenType TokenType { get; set; }
        public string RawValue { get; set; }
        public string ProcessedValue { get; set; }
        public int NumericValue { get; set; }

        public Token(PositionInfo positionInfo, TokenType tokenType, string rawValue, string processedValue, int numericValue)
        {
            this.PositionInfo = positionInfo;
            this.TokenType = tokenType;
            this.RawValue = rawValue;
            this.ProcessedValue = processedValue;
            this.NumericValue = numericValue;
        }

        public override string ToString()
        {
            return this.TokenType.ToString() + ":" + this.ProcessedValue;
        }
    }
}
