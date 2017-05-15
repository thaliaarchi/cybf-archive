using System;
using System.Collections.Generic;

namespace CyBF.BFI
{
    public class BFAssembler
    {
        private List<Node> _children;

        public BFAssembler(string code)
        {
            _children = this.BuildProgramTree(code);
        }

        public Instruction[] Compile()
        {
            List<Instruction> instructions = new List<Instruction>();

            foreach (Node child in _children)
                child.Compile(instructions);

            return instructions.ToArray();
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

                        if (stack.Count == 0)
                            throw new BFProgramError("Unmatched ] operator found.");

                        LoopNode loopNode = new LoopNode(nodes);
                        nodes = stack.Pop();
                        nodes.Add(loopNode);
                        index++;
                        break;

                    default:
                        index++;
                        break;
                }
            }

            if (stack.Count > 0)
                throw new BFProgramError("Unmatched [ operator found.");

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
