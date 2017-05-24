using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Statements.Expressions;

namespace CyBF.BFC.Model.Statements
{
    public class IterateStatement : Statement
    {
        public Variable ControlVariable { get; private set; }
        public ExpressionStatement InitializeExpression { get; private set; }
        public ExpressionStatement ConditionExpression { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }
        public ExpressionStatement NextExpression { get; private set; }

        public IterateStatement(
            Token reference, 
            Variable controlVariable, 
            ExpressionStatement initializeExpression,
            ExpressionStatement conditionExpression,
            IEnumerable<Statement> body,
            ExpressionStatement nextExpression) 
            : base(reference)
        {
            this.ControlVariable = controlVariable;
            this.InitializeExpression = initializeExpression;
            this.ConditionExpression = conditionExpression;
            this.Body = body.ToList().AsReadOnly();
            this.NextExpression = nextExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.TracePush(this.Reference);

            this.InitializeExpression.Compile(compiler);
            this.ControlVariable.Value = this.InitializeExpression.ReturnVariable.Value;

            this.ConditionExpression.Compile(compiler);
            BFObject conditionObject = this.ConditionExpression.ReturnVariable.Value;

            if (!(conditionObject.DataType is ConstInstance))
                compiler.RaiseSemanticError("Iteration statement condition expression does not evaluate to a const.");

            int conditionValue = ((ConstInstance)conditionObject.DataType).Value;

            while (conditionValue != 0)
            {
                foreach (Statement statement in this.Body)
                    statement.Compile(compiler);

                this.NextExpression.Compile(compiler);
                this.ControlVariable.Value = this.NextExpression.ReturnVariable.Value;

                this.ConditionExpression.Compile(compiler);
                conditionObject = this.ConditionExpression.ReturnVariable.Value;
                conditionValue = ((ConstInstance)conditionObject.DataType).Value;
            }

            compiler.TracePop();
        }
    }
}
