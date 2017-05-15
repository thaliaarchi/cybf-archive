using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Statements.Expressions;

namespace CyBF.BFC.Model.Statements
{
    public class ForLoopStatement : Statement
    {
        public Statement InitializeStatement { get; private set; }
        public ExpressionStatement ConditionExpression { get; private set; }
        public Statement StepStatement { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public ForLoopStatement(
            Token reference, 
            Statement initializeStatement, 
            ExpressionStatement conditionExpression, 
            Statement stepStatement, 
            IEnumerable<Statement> body)
            : base(reference)
        {
            this.InitializeStatement = initializeStatement;
            this.ConditionExpression = conditionExpression;
            this.StepStatement = stepStatement;
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            List<Statement> extendedBody = new List<Statement>(this.Body);
            extendedBody.Add(this.StepStatement);

            WhileLoopStatement mainLoop 
                = new WhileLoopStatement(this.Reference, this.ConditionExpression, extendedBody);

            this.InitializeStatement.Compile(compiler);
            mainLoop.Compile(compiler);
        }
    }
}
