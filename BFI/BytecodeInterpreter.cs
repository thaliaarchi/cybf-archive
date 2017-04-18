using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFI
{
    public class BytecodeInterpreter
    {
        public const int ZERO = 0;          // ZERO
        public const int INCREMENT = 1;     // INCREMENT <AMOUNT>
        public const int ADDSCALE = 2;      // ADDSCALE <FACTOR> <OFFSET>
        public const int SHIFT = 3;         // SHIFT <AMOUNT>
        public const int PRINT = 4;         // PRINT
        public const int READ = 5;          // READ
        public const int WHILE = 6;         // JUMPIFZERO <ADDRESS>
        public const int WEND = 7;          // JUMPIFSET <ADDRESS>
    }
}
