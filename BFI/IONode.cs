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

        public override List<int> Compile()
        {
            List<int> instructions = new List<int>(_code.Length);

            foreach (char c in _code)
            {
                switch (c)
                {
                    case ',':
                        instructions.Add(BytecodeInterpreter.READ);
                        break;

                    case '.':
                        instructions.Add(BytecodeInterpreter.PRINT);
                        break;

                    default:
                        break;
                }
            }

            return instructions;
        }
    }
}
