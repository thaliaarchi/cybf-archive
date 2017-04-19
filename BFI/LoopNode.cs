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

        public override void Compile(List<Instruction> program)
        {
            if (this.IsLinearizable())
            {
                ComputationNode node = (ComputationNode)_children.Single();

                foreach (int offset in node.GetAffectedOffsets())
                {
                    if (offset != 0)
                        program.Add(Instruction.AddScale(offset, node.GetIncrementAmount(offset)));
                }

                program.Add(Instruction.Zero());
            }
            else
            {
                int loopBeginAddress = program.Count;
                program.Add(Instruction.JumpIfZero(0));
                
                foreach (Node child in _children)
                    child.Compile(program);

                int loopEndAddress = program.Count;
                program.Add(Instruction.JumpIf(0));

                program[loopBeginAddress] = Instruction.JumpIfZero(loopEndAddress + 1);
                program[loopEndAddress] = Instruction.JumpIf(loopBeginAddress + 1);
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
