using CyBF.Parsing;
using CyBF.Utility;
using System;

namespace CyBF.BFIL
{
    public class BFILProgramError : CyBFException
    {
        private string _message;

        public override string Message
        {
            get
            {
                return _message;
            }
        }

        public BFILProgramError(Token referenceToken, string message)
        {
            if (referenceToken != null)
            {
                _message =
                    "Line " + referenceToken.PositionInfo.LineNumber.ToString() + "\n" +
                    referenceToken.PositionInfo.Line.TrimEnd() + "\n" +
                    new string(' ', referenceToken.PositionInfo.LinePosition) + "^\n" +
                    referenceToken.ToString() + "\n" +
                    message;
            }
            else
            {
                _message = message;
            }
        }
    }
}
