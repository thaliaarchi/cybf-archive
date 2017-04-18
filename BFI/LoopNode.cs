using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFI
{
    public class LoopNode : Node
    {
        public IReadOnlyList<Node> Children { get; private set; }

        public LoopNode(IEnumerable<Node> children)
        {
            this.Children = children.ToList().AsReadOnly();
        }

        public override List<int> Compile()
        {
            if (this.Children.Count == 1 && this.Children[0] is ComputationNode)
            {
                ComputationNode node = (ComputationNode)this.Children[0];

                if (node.NetShift == 0 && node.GetIncrementAmount(0) == -1)
                {
                    // Don't compile the code.
                    // We can emit a linearized version of the same behavior
                    // without a loop!
                }
            }

            throw new NotImplementedException();
        }
    }
}
