using System;
using System.Collections.Generic;

namespace CyBF.BFI
{
    public class IONode : Node
    {
        private string _code;

        public IONode(string code)
        {
            _code = code;
        }

        public override void Compile(List<Instruction> program)
        {
            foreach (char c in _code)
            {
                switch (c)
                {
                    case ',':
                        program.Add(Instruction.Read());
                        break;

                    case '.':
                        program.Add(Instruction.Print());
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
