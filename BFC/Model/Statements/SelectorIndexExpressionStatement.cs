using System;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class SelectorIndexExpressionStatement : ExpressionStatement
    {
        public ExpressionStatement Source { get; private set; }
        public IReadOnlyList<ExpressionStatement> IndexArguments { get; private set; }

        public SelectorIndexExpressionStatement(
            Token reference, 
            ExpressionStatement source, 
            IEnumerable<ExpressionStatement> indexArguments) 
            : base(reference)
        {
            this.Source = source;
            this.IndexArguments = indexArguments.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            this.Source.Compile(compiler);
            
            foreach (ExpressionStatement argument in this.IndexArguments)
                argument.Compile(compiler);

            BFObject sourceObject = this.Source.ReturnVariable.Value;

            IEnumerable<BFObject> indexObjects = this.IndexArguments.Select(
                arg => arg.ReturnVariable.Value);

            compiler.TracePush(this.Reference);

            IEnumerable<BFObject> functionArgumentObjects =
                new BFObject[] { sourceObject }.Concat(indexObjects);

            List<SelectorDefinition> matches =
                compiler.MatchFunction("selector", functionArgumentObjects.Select(arg => arg.DataType))
                .Select(f => (SelectorDefinition)f).ToList();

            if (matches.Count == 0)
            {
                compiler.RaiseSemanticError("No matching selector definitions found:\n" + this.BuildSignature(sourceObject, indexObjects));
            }

            if (matches.Count > 1)
            {
                List<Token> references = new List<Token>();
                references.Add(this.Reference);
                references.AddRange(matches.Select(m => m.ReferenceToken));
                
                throw new SemanticError("Ambiguous selector call:\n" + this.BuildSignature(sourceObject, indexObjects), references);
            }

            SelectorDefinition definition = matches.Single();
            this.ReturnVariable.Value = definition.Compile(compiler, sourceObject, indexObjects);

            compiler.TracePop();
        }

        private string BuildSignature(BFObject sourceObject, IEnumerable<BFObject> indexObjects)
        {
            string sourceString = sourceObject.DataType.ToString();
            string argumentString = string.Join(", ", indexObjects.Select(obj => obj.DataType.ToString()));
            return sourceString + "[" + argumentString + "]";
        }
    }
}
