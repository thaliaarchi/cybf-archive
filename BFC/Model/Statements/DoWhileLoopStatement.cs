using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements
{
    public class DoWhileLoopStatement : Statement
    {
        public ExpressionStatement ConditionExpression { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public DoWhileLoopStatement(Token reference, ExpressionStatement conditionExpression, IEnumerable<Statement> body) 
            : base(reference)
        {
            this.ConditionExpression = conditionExpression;
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            WhileLoopStatement continuation = new WhileLoopStatement(this.Reference, this.ConditionExpression, this.Body);
            continuation.Compile(compiler);
        }
    }
}
