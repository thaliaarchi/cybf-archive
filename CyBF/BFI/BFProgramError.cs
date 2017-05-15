using CyBF.Utility;
using System;

namespace CyBF.BFI
{
    public class BFProgramError : CyBFException
    {
        public BFProgramError(string message) 
            : base(message)
        {
        }
    }
}
