using CyBF.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyBF.BFC.Model
{
    public class SemanticError : Exception
    {
        private string _message;

        public override string Message
        {
            get
            {
                return _message;
            }
        }

        public SemanticError(string message, IEnumerable<Token> tokens)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(message);
            builder.AppendLine();

            foreach (Token token in tokens)
            {
                builder.AppendLine(token.PositionInfo.Source);
                builder.AppendLine("Line " + token.PositionInfo.LineNumber.ToString());
                builder.AppendLine(token.PositionInfo.Line.TrimEnd());
                builder.AppendLine(new string(' ', token.PositionInfo.LinePosition) + "^");
                builder.AppendLine();
            }

            _message = builder.ToString();
        }

        public SemanticError(string message, params Token[] tokens)
            : this(message, (IEnumerable<Token>) tokens)
        {
        }
    }
}
