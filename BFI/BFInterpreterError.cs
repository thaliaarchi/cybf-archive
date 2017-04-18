using System;

namespace CyBF.BFI
{
    public class BFInterpreterError : Exception
    {
        public BFInterpreterError(string message) 
            : base(message)
        {
        }
    }
}
