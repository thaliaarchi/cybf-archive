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
        private Stack<Token> _trace = new Stack<Token>();

        // TODO: Just make a public property of IReadOnlyList. Or, maybe even just List.
        // String "id"s could simply be indexes into this list. 
        private int _cachedStringAutonum = 0;
        private Dictionary<int, CyBFString> _cachedStringLiterals = new Dictionary<int, CyBFString>();

        public DefinitionLibrary Library { get; private set; }
        public BFObject CurrentAllocatedObject { get; private set; }

        public BFCompiler(DefinitionLibrary library)
        {
            this.Library = library;
        }

        public void Write(string code)
        {
            _compiledCode.Append(code);
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
            return this.Library.MatchFunction(name, arguments);
        }

        public List<TypeDefinition> MatchType(string name, IEnumerable<TypeInstance> arguments)
        {
            return this.Library.MatchType(name, arguments);
        }
        
        public void MoveToObject(BFObject bfobject)
        {
            if (bfobject.DataType.Size() > 0 &&
                bfobject != this.CurrentAllocatedObject)
            {
                if (this.CurrentAllocatedObject != null)
                    this.CurrentAllocatedObject.UndoOffsets(this);

                this.Write(bfobject.AllocationId + " ");

                bfobject.ApplyOffsets(this);

                this.CurrentAllocatedObject = bfobject;
            }
        }

        public BFObject MakeAndMoveToObject(TypeInstance dataType, string allocationIdPrefix = null)
        {
            BFObject bfobject = allocationIdPrefix != null ?
                new BFObject(dataType, allocationIdPrefix) : new BFObject(dataType);

            int size = dataType.Size();

            if (size > 0)
            {
                if (this.CurrentAllocatedObject != null)
                    this.CurrentAllocatedObject.UndoOffsets(this);

                this.Write("@" + bfobject.AllocationId + ":" + size.ToString());

                this.CurrentAllocatedObject = bfobject;
            }

            return bfobject;
        }

        public bool ContainsCachedString(int id)
        {
            return _cachedStringLiterals.ContainsKey(id);
        }

        public CyBFString GetCachedString(int id)
        {
            return _cachedStringLiterals[id];
        }

        public int CacheString(CyBFString value)
        {
            int id = _cachedStringAutonum++;
            _cachedStringLiterals[id] = value;
            return id;
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
