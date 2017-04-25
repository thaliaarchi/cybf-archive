using System;

namespace CyBF.Utility
{
    public class CyBFException : Exception
    {
        public CyBFException()
            : base()
        {
        }

        public CyBFException(string message)
            : base(message)
        {
        }
    }
}
