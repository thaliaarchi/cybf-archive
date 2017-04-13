using System;

namespace CyBF.Parsing
{
    public class LexicalError : Exception
    {
        public LexicalError(string message)
            :base(message)
        {
        }
    }
}
