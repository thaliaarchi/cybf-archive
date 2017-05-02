using System;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Functions;

namespace CyBF.BFC.Model.Statements
{
    public class SelectorIndexStatement : Statement
    {
        public Variable Subject { get; private set; }
        public IReadOnlyList<Variable> IndexArguments { get; private set; }
        public Variable ReturnValue { get; private set; }

        public SelectorIndexStatement(
            Token reference, 
            Variable subject, 
            IEnumerable<Variable> indexArguments,
            Variable returnValue) 
            : base(reference)
        {
            this.Subject = subject;
            this.IndexArguments = indexArguments.ToList().AsReadOnly();
            this.ReturnValue = returnValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.TracePush(this.Reference);

            IEnumerable<Variable> functionArguments =
                new Variable[] { this.Subject }.Concat(this.IndexArguments);

            List<SelectorDefinition> matches =
                compiler.MatchFunction("selector", functionArguments.Select(arg => arg.Value.DataType))
                .Select(f => (SelectorDefinition)f).ToList();

            if (matches.Count == 0)
                compiler.RaiseSemanticError("No matching selector definitions found:\n" + this.BuildSignature());

            if (matches.Count > 1)
            {
                List<Token> references = new List<Token>();
                references.Add(this.Reference);
                references.AddRange(matches.Select(m => m.ReferenceToken));
                
                throw new SemanticError("Ambiguous selector call:\n" + this.BuildSignature(), references);
            }

            SelectorDefinition definition = matches.Single();
            definition.Compile(compiler, functionArguments.Select(v => v.Value));

            this.ReturnValue.Value = definition.ReturnValue.Value;

            compiler.TracePop();
        }

        private string BuildSignature()
        {
            string subjectString = this.Subject.Value.DataType.ToString();
            string argumentString = string.Join(", ", this.IndexArguments.Select(a => a.Value.DataType.ToString()));
            return this.Subject.Value + " [" + argumentString + "]";
        }
    }
}
