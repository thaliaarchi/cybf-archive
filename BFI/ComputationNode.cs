using CyBF.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFI
{
    public class ComputationNode : Node
    {
        private DefaultDictionary<int, int> _offsetIncrements;
        private int _netShift;

        public int NetShift { get { return _netShift; } }
        
        public ComputationNode(string code)
        {
            BuildOffsetIncrementTable(code);
        }

        public List<int> GetAffectedOffsets()
        {
            return _offsetIncrements.Keys.ToList();
        }

        public int GetIncrementAmount(int offset)
        {
            return _offsetIncrements[offset];
        }

        public override void Compile(List<Instruction> program)
        {
            List<int> offsets = this.GetAffectedOffsets();

            int currentOffset = 0;
            int shiftAmount;
            int incrementAmount;

            foreach (int offset in offsets)
            {
                shiftAmount = offset - currentOffset;

                if (shiftAmount != 0)
                {
                    program.Add(Instruction.Shift(shiftAmount));
                    currentOffset += shiftAmount;
                }

                incrementAmount = this.GetIncrementAmount(offset);
                program.Add(Instruction.Add(incrementAmount));
            }

            shiftAmount = this.NetShift - currentOffset;

            if (shiftAmount != 0)
            {
                program.Add(Instruction.Shift(shiftAmount));
                currentOffset += shiftAmount;
            }
        }

        private void BuildOffsetIncrementTable(string code)
        {
            _offsetIncrements = new DefaultDictionary<int, int>();
            int currentOffset = 0;

            foreach (char c in code)
            {
                switch (c)
                {
                    case '+':
                        _offsetIncrements[currentOffset]++;
                        break;

                    case '-':
                        _offsetIncrements[currentOffset]--;
                        break;

                    case '<':
                        currentOffset--;
                        break;

                    case '>':
                        currentOffset++;
                        break;

                    default:
                        break;
                }
            }

            _netShift = currentOffset;
        }
    }
}
