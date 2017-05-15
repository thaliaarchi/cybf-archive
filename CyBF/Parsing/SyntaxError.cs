using CyBF.Utility;
using System;

namespace CyBF.Parsing
{
    public class SyntaxError : CyBFException
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
                token.PositionInfo.Source + "\n" +
                "Line " + token.PositionInfo.LineNumber.ToString() + "\n" +
                token.PositionInfo.Line.TrimEnd() + "\n" +
                new string(' ', token.PositionInfo.LinePosition) + "^\n" +
                "Expected " + expected + "\n" +
                "Received " + token.ToString();
        }
    }
}
