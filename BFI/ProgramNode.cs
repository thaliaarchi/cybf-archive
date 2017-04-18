using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFI
{
    public class ProgramNode : Node
    {
        public ProgramNode(string code)
        {
        }

        public override List<int> Compile()
        {
            throw new NotImplementedException();
        }

        private List<Node> BuildProgramTree(string code)
        {
            Stack<List<Node>> stack = new Stack<List<Node>>();
            List<Node> nodes = new List<Node>();

            int index = 0;
            string snippet;

            while (index < code.Length)
            {
                switch (code[index])
                {
                    case '+':
                    case '-':
                    case '<':
                    case '>':
                        snippet = SelectSnippet(code, ref index, '.', ',', '[', ']');
                        nodes.Add(new ComputationNode(snippet));
                        break;

                    case '.':
                    case ',':
                        snippet = SelectSnippet(code, ref index, '+', '-', '<', '>', '[', ']');
                        nodes.Add(new IONode(snippet));
                        break;

                    case '[':
                        stack.Push(nodes);
                        nodes = new List<Node>();
                        index++;
                        break;

                    case ']':

                        LoopNode loopNode = new LoopNode(nodes);

                        if (stack.Count == 0)
                            throw new BFInterpreterError("Unmatched ] operator found.");

                        nodes = stack.Pop();
                        nodes.Add(loopNode);
                        break;

                    default:
                        break;
                }
            }

            if (stack.Count > 0)
                throw new BFInterpreterError("Unmatched [ operator found.");

            return nodes;
        }

        private string SelectSnippet(string code, ref int index, params char[] terminatingChars)
        {
            int startIndex = index;

            int endIndex = code.IndexOfAny(terminatingChars, startIndex);
            endIndex = (endIndex == -1 ? code.Length : endIndex);

            string snippet = code.Substring(startIndex, endIndex - startIndex);

            index = endIndex;

            return snippet;
        }
    }
}
