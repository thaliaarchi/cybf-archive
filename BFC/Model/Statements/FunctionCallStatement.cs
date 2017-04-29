using CyBF.Parsing;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;

namespace CyBF.BFC.Model.Statements
{
    public class FunctionCallStatement : Statement
    {
        public string FunctionName { get; private set; }
        public IReadOnlyList<Variable> Arguments { get; private set; }
        public Variable ReturnValue { get; private set; }

        public FunctionCallStatement(
            Token reference, 
            string functionName, 
            IEnumerable<Variable> arguments,
            Variable returnValue)
            : base(reference)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments.ToList().AsReadOnly();
            this.ReturnValue = returnValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.TracePush(this.Reference);

            List<FunctionDefinition> matches = compiler.MatchFunction(this.FunctionName, this.Arguments.Select(v => v.Value.DataType));

            if (matches.Count == 0)
                compiler.RaiseSemanticError("No matching function definitions found.");

            if (matches.Count > 1)
            {
                IEnumerable<ProcedureDefinition> procedures = matches
                    .Where(m => m is ProcedureDefinition)
                    .Select(m => m as ProcedureDefinition);

                List<Token> references = new List<Token>();

                references.Add(this.Reference);

                foreach (ProcedureDefinition proc in procedures)
                    references.Add(proc.Reference);

                string signature = this.BuildSignature();
                throw new SemanticError("Ambiguous function call: " + signature, references);
            }

            FunctionDefinition definition = matches.Single();
            definition.Compile(compiler, this.Arguments.Select(v => v.Value));

            this.ReturnValue.Value = definition.ReturnValue.Value;

            compiler.TracePop();
        }

        private string BuildSignature()
        {
            string argstring = string.Join(", ", this.Arguments.Select(a => a.Value.DataType.ToString()));
            return this.FunctionName + "(" + argstring + ")";
        }
    }
}
