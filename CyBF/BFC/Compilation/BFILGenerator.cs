using CyBF.BFIL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Compilation
{
    /*
        This class is a bit of a hack in order to bypass the BFIL parser.
        Completely underestimated how much BFIL code even a tiny BFC program is likely to generate.
        Luckily, all exception conditions raised by the BFIL component are checked for in BFC too.
        Hence, having null reference tokens shouldn't cause problems. 

        If needed, we can modify this to actually generate BFIL code and forge
        reference tokens that align to it. 
    */
    public class BFILGenerator
    {
        private List<BFILStatement> _statements = new List<BFILStatement>();
        private Stack<List<BFILStatement>> _stack = new Stack<List<BFILStatement>>();

        public BFILProgram MakeBFILProgram()
        {
            if (_stack.Count > 0)
                throw new InvalidOperationException("Generator loop stack is not empty.");

            return new BFILProgram(_statements);
        }

        // Loop commands [ and ] are technically allowed here. 
        // They'll only cause problems if we attempt to reference variables from within them.
        // In such cases, use Begin/EndCheckedLoop. 
        public void AppendBF(string code)
        {
            BFILCommandStatement lastCommandStatement = null;

            if (_statements.Count > 0)
                lastCommandStatement = _statements.LastOrDefault() as BFILCommandStatement;

            if (lastCommandStatement != null)
            {
                _statements[_statements.Count - 1]
                    = new BFILCommandStatement(null, lastCommandStatement.Commands + code);
            }
            else
            {
                _statements.Add(new BFILCommandStatement(null, code));
            }
        }

        public void DeclareVariable(string variableName, int size)
        {
            _statements.Add(new BFILDeclarationStatement(null, variableName, size));
        }

        public void ReferenceVariable(string variableName)
        {
            _statements.Add(new BFILReferenceStatement(null, variableName));
        }

        public void BeginCheckedLoop()
        {
            _stack.Push(_statements);
            _statements = new List<BFILStatement>();
        }

        public void EndCheckedLoop()
        {
            List<BFILStatement> body = _statements;
            _statements = _stack.Pop();
            _statements.Add(new BFILLoopStatement(null, body));
        }

        public void WriteData(IEnumerable<byte> data)
        {
            _statements.Add(new BFILWriteStatement(null, data));
        }
    }
}
