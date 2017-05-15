using CyBF.Utility;
using System;

namespace CyBF.Parsing
{
    public class LexicalError : CyBFException
    {
        public LexicalError(string message)
            :base(message)
        {
        }
    }
}
