using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFI
{
    public class LoopNode : Node
    {
        private List<Node> _children;

        public LoopNode(IEnumerable<Node> children)
        {
            _children = children.ToList();
        }

        public override void Compile(List<int> programInstructions)
        {
            if (this.IsLinearizable())
            {
                ComputationNode node = (ComputationNode)_children.Single();

                foreach (int offset in node.GetAffectedOffsets())
                {
                    if (offset != 0)
                    {
                        programInstructions.Add(BytecodeInterpreter.ADDSCALE);
                        programInstructions.Add(node.GetIncrementAmount(offset));
                        programInstructions.Add(offset);
                    }
                }

                programInstructions.Add(BytecodeInterpreter.ZERO);
            }
            else
            {
                int loopBeginInstruction = programInstructions.Count;
                programInstructions.Add(BytecodeInterpreter.JUMPIFZERO);
                programInstructions.Add(0);
                int loopBodyBegin = programInstructions.Count;

                foreach (Node child in _children)
                    child.Compile(programInstructions);

                int loopEndInstruction = programInstructions.Count;
                programInstructions.Add(BytecodeInterpreter.JUMPIF);
                programInstructions.Add(0);
                int loopFollowBegin = programInstructions.Count;

                programInstructions[loopBeginInstruction + 1] = loopFollowBegin;
                programInstructions[loopEndInstruction + 1] = loopBodyBegin;
            }
        }

        private bool IsLinearizable()
        {
            if (_children.Count == 1 && _children[0] is ComputationNode)
            {
                ComputationNode node = (ComputationNode)_children[0];

                if (node.NetShift == 0 && node.GetIncrementAmount(0) == -1)
                    return true;
            }

            return false;
        }
    }
}
