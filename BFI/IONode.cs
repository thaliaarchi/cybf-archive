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

        public override void Compile(List<int> programInstructions)
        {
            foreach (char c in _code)
            {
                switch (c)
                {
                    case ',':
                        programInstructions.Add(BytecodeInterpreter.READ);
                        break;

                    case '.':
                        programInstructions.Add(BytecodeInterpreter.PRINT);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
