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

            compiler.Write("[");

            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            controlObject = GetControlObject(compiler);
            compiler.MoveToObject(controlObject);

            compiler.Write("]");
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
                compiler.Write("[-]");

                BFObject tempObject = compiler.AllocateAndMoveToObject(new ByteInstance());
                compiler.Write("[-]");

                compiler.MoveToObject(conditionObject);
                compiler.Write("[");
                compiler.MoveToObject(controlObject);
                compiler.Write("+");
                compiler.MoveToObject(tempObject);
                compiler.Write("+");
                compiler.MoveToObject(conditionObject);
                compiler.Write("-");
                compiler.Write("]");

                compiler.MoveToObject(tempObject);
                compiler.Write("[");
                compiler.MoveToObject(conditionObject);
                compiler.Write("+");
                compiler.MoveToObject(tempObject);
                compiler.Write("-");
                compiler.Write("]");

                return controlObject;
            }
        }
    }
}
