using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Statements.Expressions;

namespace CyBF.BFC.Model.Statements
{
    public class WhileLoopStatement : Statement
    {
        public ExpressionStatement ConditionExpression { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public WhileLoopStatement(Token reference, ExpressionStatement conditionExpression, IEnumerable<Statement> body) 
            : base(reference)
        {
            this.ConditionExpression = conditionExpression;
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            BFObject controlObject = GetControlObject(compiler);
            compiler.MoveToObject(controlObject);

            compiler.BeginCheckedLoop();

            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            controlObject = GetControlObject(compiler);
            compiler.MoveToObject(controlObject);

            compiler.EndCheckedLoop();
        }

        private BFObject GetControlObject(BFCompiler compiler)
        {
            this.ConditionExpression.Compile(compiler);
            BFObject conditionObject = this.ConditionExpression.ReturnVariable.Value;

            if (!(conditionObject.DataType is ByteInstance))
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError(string.Format(
                    "Condition expression evaluates to '{0}'. Must evaluate to Byte.",
                    conditionObject.DataType.ToString()));
            }

            if (!this.ConditionExpression.IsVolatile())
            {
                return conditionObject;
            }
            else
            {
                BFObject controlObject = compiler.AllocateAndMoveToObject(new ByteInstance());
                compiler.AppendBF("[-]");

                BFObject tempObject = compiler.AllocateAndMoveToObject(new ByteInstance());
                compiler.AppendBF("[-]");

                compiler.MoveToObject(conditionObject);
                compiler.BeginCheckedLoop();
                compiler.MoveToObject(controlObject);
                compiler.AppendBF("+");
                compiler.MoveToObject(tempObject);
                compiler.AppendBF("+");
                compiler.MoveToObject(conditionObject);
                compiler.AppendBF("-");
                compiler.EndCheckedLoop(); ;

                compiler.MoveToObject(tempObject);
                compiler.BeginCheckedLoop();
                compiler.MoveToObject(conditionObject);
                compiler.AppendBF("+");
                compiler.MoveToObject(tempObject);
                compiler.AppendBF("-");
                compiler.EndCheckedLoop();

                return controlObject;
            }
        }
    }
}
