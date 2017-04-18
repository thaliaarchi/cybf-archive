using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFI
{
    public class BytecodeInterpreter
    {
        public const int ZERO = 0;              // ZERO
        public const int ADD = 1;               // ADD <AMOUNT>                     <-- ADD BYTE
        public const int ADDSCALE = 2;          // ADDSCALE <FACTOR> <OFFSET>       <-- ADDSCALE BYTE INTEGER
        public const int SHIFT = 3;             // SHIFT <AMOUNT>                   <-- SHIFT INTEGER
        public const int PRINT = 4;             // PRINT
        public const int READ = 5;              // READ
        public const int JUMPIFZERO = 6;        // JUMPIFZERO <ADDRESS>             <-- JUMPIFZERO INTEGER
        public const int JUMPIF = 7;            // JUMPIF <ADDRESS>                 <-- JUMPIF INTEGER
        
        public void Run(int[] instructions)
        {
            List<int> memory = new List<int>(30000);

            int ip = 0;
            int mp = 0;

            // I don't think I like this design as much as I thought I would.

            while (ip < instructions.Length)
            {
                switch (instructions[ip])
                {
                    case ZERO:
                        memory[mp] = 0;
                        ip++;
                        break;

                    case ADD:
                        memory[mp] += instructions[ip + 1];
                        ip += 2;
                        break;

                    case ADDSCALE:
                        memory[instructions[ip + 2]] += memory[mp] * instructions[ip + 1];
                        ip += 3;
                        break;

                    case SHIFT:
                        break;

                    case PRINT:
                        break;

                    case READ:
                        break;

                    case JUMPIFZERO:
                        break;

                    case JUMPIF:
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
