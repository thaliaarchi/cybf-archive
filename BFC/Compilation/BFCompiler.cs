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
        private StringBuilder _compiledCode = new StringBuilder();
        private Dictionary<int, CyBFString> _cachedStringLiterals = new Dictionary<int, CyBFString>();
        private DefinitionLibrary _definitions = new DefinitionLibrary();
        private Stack<Token> _trace = new Stack<Token>();
        private int _allocationAutonum = 1;

        public bool Verbose { get; set; }

        public BFCompiler()
        {
            this.Verbose = false;
        }

        public void Write(string code)
        {
            _compiledCode.Append(code);
        }

        public void WriteLine(string code)
        {
            _compiledCode.AppendLine(code);
        }

        public void WriteLine()
        {
            _compiledCode.AppendLine();
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
            return _compiledCode.ToString();
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
            return prefix + "_" + (_allocationAutonum++).ToString();
        }
        
        public bool ContainsCachedString(int id)
        {
            return _cachedStringLiterals.ContainsKey(id);
        }

        public CyBFString GetCachedString(int id)
        {
            return _cachedStringLiterals[id];
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
