using CyBF.BFC.Model;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Definitions;
using CyBF.BFC.Model.Types.Instances;
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
        private HashSet<object> _currentlyCompilingDefinitions = new HashSet<object>();

        public DefinitionLibrary<TypeDefinition> TypeLibrary { get; private set; }
        public DefinitionLibrary<FunctionDefinition> FunctionLibrary { get; private set; }
        public BFObject CurrentAllocatedObject { get; private set; }

        public BFCompiler(DefinitionLibrary<TypeDefinition> typeLibrary, DefinitionLibrary<FunctionDefinition> functionLibrary)
        {
            this.TypeLibrary = typeLibrary;
            this.FunctionLibrary = functionLibrary;
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

        private T ResolveDefinition<T>(
            string name,
            IEnumerable<TypeInstance> arguments,
            DefinitionLibrary<T> library,
            string noMatchErrorMessage,
            string multipleMatchErrorMessage)
            where T : Definition
        {
            List<T> matches = library.Match(name, arguments);

            if (matches.Count == 0)
            {
                RaiseSemanticError(noMatchErrorMessage);
            }
            else if (matches.Count > 1)
            {
                IEnumerable<Token> definitionReferences = matches
                    .Select(d =>
                        (d as ProcedureDefinition)?.Reference ??
                        (d as StructDefinition)?.Reference)
                    .Where(t => t != null);

                foreach (Token reference in definitionReferences)
                    this.TracePush(reference);

                RaiseSemanticError(multipleMatchErrorMessage);
            }

            return matches.Single();
        }

        public TypeDefinition ResolveType(string name, IEnumerable<TypeInstance> arguments)
        {
            string signature = "[" + name + " " + string.Join(" ", arguments) + "]";

            TypeDefinition type = this.ResolveDefinition(
                name, arguments, this.TypeLibrary,
                "No matching type definitions found:\n" + signature,
                "Ambiguous type constructor:\n" + signature);

            return type;
        }

        public FunctionDefinition ResolveFunction(string name, IEnumerable<BFObject> arguments)
        {
            IEnumerable<TypeInstance> argumentTypes = arguments.Select(arg => arg.DataType);
            string signature = name + "(" + string.Join(", ", argumentTypes) + ")";

            FunctionDefinition function = this.ResolveDefinition(
                name, arguments.Select(arg => arg.DataType), this.FunctionLibrary,
                "No matching function definitions found:\n" + signature,
                "Ambiguous function call:\n" + signature);

            return function;
        }

        public FunctionDefinition ResolveMethod(string name, BFObject source, IEnumerable<BFObject> arguments)
        {
            IEnumerable<TypeInstance> methodArgumentTypes = arguments.Select(arg => arg.DataType);

            string signature = string.Format("{0}.{1}({2})",
                source.DataType, name, string.Join(", ", methodArgumentTypes));

            IEnumerable<TypeInstance> completeArgumentTypes =
                new TypeInstance[] { source.DataType }.Concat(methodArgumentTypes);

            return this.ResolveDefinition(
                name, completeArgumentTypes, source.DataType.MethodLibrary,
                "No matching method definitions found:\n" + signature,
                "Ambiguous method invocation:\n" + signature);
        }

        public void MoveToObject(BFObject bfobject)
        {
            if (bfobject.DataType.Size() == 0)
                throw new ArgumentException("Cannot move to zero-sized object.");

            List<BFObject> derivationList = bfobject.GetDerivationList();

            while (this.CurrentAllocatedObject != null 
                && !derivationList.Contains(this.CurrentAllocatedObject))
            {
                this.CurrentAllocatedObject.Offset.Dereference(this);
                this.CurrentAllocatedObject = this.CurrentAllocatedObject.Parent;
            }

            int commonRootIndex;

            if (this.CurrentAllocatedObject == null)
            {
                this.Write(derivationList[0].AllocationId + " ");
                this.CurrentAllocatedObject = derivationList[0];
                commonRootIndex = 0;
            }
            else
            {
                commonRootIndex = derivationList.IndexOf(this.CurrentAllocatedObject);
            }

            for (int i = commonRootIndex + 1; i < derivationList.Count; i++)
            {
                derivationList[i].Offset.Reference(this);
                this.CurrentAllocatedObject = derivationList[i];
            }
        }

        public BFObject AllocateAndMoveToObject(TypeInstance dataType, string allocationIdPrefix = null)
        {
            int size = dataType.Size();

            if (size == 0)
                throw new ArgumentException("Cannot allocate zero-sized object.");

            BFObject bfobject = allocationIdPrefix != null ?
                new BFObject(dataType, allocationIdPrefix) : new BFObject(dataType);
            
            while (this.CurrentAllocatedObject != null)
            {
                this.CurrentAllocatedObject.Offset.Dereference(this);
                this.CurrentAllocatedObject = this.CurrentAllocatedObject.Parent;
            }
            
            this.Write("@" + bfobject.AllocationId + ":" + size.ToString());

            this.CurrentAllocatedObject = bfobject;

            return bfobject;
        }

        public RecursionChecker BeginRecursionCheck(object item)
        {
            if (!_currentlyCompilingDefinitions.Add(item))
                RaiseSemanticError("Recursive definitions are not supported.");

            return new RecursionChecker(this, item);
        }

        public void EndRecursionCheck(object item)
        {
            _currentlyCompilingDefinitions.Remove(item);
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
