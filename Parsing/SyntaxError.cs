using System;

namespace CyBF.Parsing
{
    public class SyntaxError : Exception
    {
        private string _message;

        public override string Message
        {
            get
            {
                return _message;
            }
        }

        public SyntaxError(Token token, string expected)
        {
            _message =
                token.PositionInfo.SourceName + "\n" +
                "Line " + token.PositionInfo.LineNumber.ToString() + "\n" +
                token.PositionInfo.Line + "\n" +
                new string(' ', token.PositionInfo.LinePosition) + "^\n" +
                "Expected " + expected + "\n" +
                "Received " + token.ToString();
        }
    }
}
