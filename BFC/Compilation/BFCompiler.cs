using CyBF.BFC.Model;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFC.Compilation
{
    public class BFCompiler
    {
        private StringBuilder _compiledBFA = new StringBuilder();
        private DefinitionLibrary _definitions = new DefinitionLibrary();
        private Stack<Token> _trace = new Stack<Token>();
        private int _allocationId = 0;

        public bool Verbose { get; set; }

        public BFCompiler()
        {
            this.Verbose = false;
        }

        public void Write(string code)
        {
            _compiledBFA.Append(code);
        }

        public void WriteLine(string code)
        {
            _compiledBFA.AppendLine(code);
        }

        public void WriteComments(string comments)
        {
            if (this.Verbose)
            {
                using (var reader = new StringReader(comments))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                        this.WriteLine("`" + line);
                }
            }
        }

        public string GetCode()
        {
            return _compiledBFA.ToString();
        }

        public void TracePush(Token token)
        {
            _trace.Push(token);
        }

        public void TracePop()
        {
            _trace.Pop();
        }
        
        public List<FunctionDefinition> MatchFunction(string name, IEnumerable<TypeInstance> arguments)
        {
            return _definitions.MatchFunction(name, arguments);
        }

        public List<TypeDefinition> MatchType(string name, IEnumerable<TypeInstance> arguments)
        {
            return _definitions.MatchType(name, arguments);
        }

        public string NewAllocationId(string prefix)
        {
            return prefix + (_allocationId++).ToString();
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
