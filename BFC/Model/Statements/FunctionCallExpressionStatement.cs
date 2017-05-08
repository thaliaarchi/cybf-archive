using CyBF.Parsing;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class FunctionCallExpressionStatement : ExpressionStatement
    {
        public string FunctionName { get; private set; }
        public IReadOnlyList<ExpressionStatement> Arguments { get; private set; }

        public FunctionCallExpressionStatement(
            Token reference, 
            string functionName, 
            IEnumerable<ExpressionStatement> arguments)
            : base(reference)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            foreach (ExpressionStatement argument in this.Arguments)
                argument.Compile(compiler);

            IEnumerable<BFObject> argumentObjects = this.Arguments.Select(arg => arg.ReturnVariable.Value);

            compiler.TracePush(this.Reference);

            List<FunctionDefinition> matches = compiler.MatchFunction(this.FunctionName, argumentObjects.Select(arg => arg.DataType));

            if (matches.Count == 0)
                compiler.RaiseSemanticError("No matching function definitions found:\n" + this.BuildSignature(argumentObjects));

            if (matches.Count > 1)
            {
                IEnumerable<ProcedureDefinition> procedures = matches
                    .Where(m => m is ProcedureDefinition)
                    .Select(m => m as ProcedureDefinition);

                List<Token> references = new List<Token>();
                references.Add(this.Reference);
                references.AddRange(procedures.Select(p => p.Reference));
                
                throw new SemanticError("Ambiguous function call:\n" + this.BuildSignature(argumentObjects), references);
            }

            FunctionDefinition definition = matches.Single();
            this.ReturnVariable.Value = definition.Compile(compiler, argumentObjects);

            compiler.TracePop();
        }

        private string BuildSignature(IEnumerable<BFObject> argumentObjects)
        {
            string argumentString = string.Join(", ", argumentObjects.Select(arg => arg.DataType.ToString()));
            return this.FunctionName + "(" + argumentString + ")";
        }
    }
}
