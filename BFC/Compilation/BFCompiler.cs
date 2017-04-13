using CyBF.BFC.Model.Statements;
using CyBF.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFC.Compilation
{
    public class BFCompiler
    {
        private Stack<Token> _trace = new Stack<Token>();

        public void TracePush(Token token)
        {
            _trace.Push(token);
        }

        public void TracePop()
        {
            _trace.Pop();
        }

        public void CompileStatement(Statement statement)
        {
            throw new NotImplementedException();
        }

        public void RaiseSemanticError(string message)
        {
            List<Token> tokens = new List<Token>();

            while (_trace.Count > 0)
                tokens.Add(_trace.Pop());

            throw new SemanticError(message, tokens);
        }
    }
}
