using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Statements;
using System.Linq;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Functions
{
    public class ProcedureDefinition : FunctionDefinition
    {
        public Token Reference { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }
        public ExpressionStatement ReturnExpression { get; private set; }

        public ProcedureDefinition(
            Token reference,
            string name,
            IEnumerable<FunctionParameter> parameters,
            IEnumerable<Statement> body,
            ExpressionStatement returnExpression)
            : base(name, parameters)
        {
            this.Reference = reference;
            this.Body = body.ToList().AsReadOnly();
            this.ReturnExpression = returnExpression;
        }

        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.Reference);
            
            this.ApplyArguments(compiler, arguments);

            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            this.ReturnExpression.Compile(compiler);
            BFObject returnValue = this.ReturnExpression.ReturnVariable.Value;
            
            compiler.TracePop();

            return returnValue;
        }
    }
}
