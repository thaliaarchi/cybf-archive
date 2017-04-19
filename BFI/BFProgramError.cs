using System;

namespace CyBF.BFI
{
    public class BFProgramError : Exception
    {
        public BFProgramError(string message) 
            : base(message)
        {
        }
    }
}
