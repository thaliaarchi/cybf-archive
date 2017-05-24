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
    public class IfStatement : Statement
    {
        public ExpressionStatement Condition { get; private set; }
        public IReadOnlyList<Statement> ConditionalBody { get; private set; }
        public IReadOnlyList<Statement> ElseBody { get; private set; }

        public IfStatement(Token reference, ExpressionStatement condition, IEnumerable<Statement> conditionalBody, IEnumerable<Statement> elseBody) 
            : base(reference)
        {
            this.Condition = condition;
            this.ConditionalBody = conditionalBody.ToList().AsReadOnly();
            this.ElseBody = elseBody.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            this.Condition.Compile(compiler);
            BFObject conditionObject = this.Condition.ReturnVariable.Value;

            if (conditionObject.DataType is ConstInstance)
            {
                CompileConst(compiler, conditionObject);
            }
            else
            {
                if (!(conditionObject.DataType is ByteInstance))
                {
                    compiler.TracePush(this.Reference);
                    compiler.RaiseSemanticError(string.Format(
                        "Condition expression evaluates to '{0}'. Must evaluate to Const or Byte.",
                        conditionObject.DataType.ToString()));
                }

                if (this.ElseBody.Count == 0)
                    CompileByteIf(compiler, conditionObject);
                else
                    CompileByteIfElse(compiler, conditionObject);
            }
        }

        private void CompileConst(BFCompiler compiler, BFObject conditionObject)
        {
            int conditionalValue = ((ConstInstance)conditionObject.DataType).Value;

            if (conditionalValue != 0)
            {
                foreach (Statement statement in this.ConditionalBody)
                    statement.Compile(compiler);
            }
            else
            {
                foreach (Statement statement in this.ElseBody)
                    statement.Compile(compiler);
            }
        }

        private void CompileByteIf(BFCompiler compiler, BFObject conditionObject)
        {
            BFObject c = compiler.CopyByte(conditionObject);

            compiler.MoveToObject(c);
            compiler.BeginCheckedLoop();

            foreach (Statement statement in this.ConditionalBody)
                statement.Compile(compiler);

            compiler.MoveToObject(c);
            compiler.AppendBF("[-]");
            compiler.EndCheckedLoop();
        }

        private void CompileByteIfElse(BFCompiler compiler, BFObject conditionObject)
        {
            BFObject c = compiler.CopyByte(conditionObject);
            BFObject e = compiler.AllocateAndMoveToObject(new ByteInstance());
            compiler.AppendBF("[-]+");

            compiler.MoveToObject(c);
            compiler.BeginCheckedLoop();

            foreach (Statement statement in this.ConditionalBody)
                statement.Compile(compiler);

            compiler.MoveToObject(e);
            compiler.AppendBF("[-]");
            compiler.MoveToObject(c);
            compiler.AppendBF("[-]");
            compiler.EndCheckedLoop();

            compiler.MoveToObject(e);
            compiler.BeginCheckedLoop();

            foreach (Statement statement in this.ElseBody)
                statement.Compile(compiler);

            compiler.MoveToObject(e);
            compiler.AppendBF("[-]");
            compiler.EndCheckedLoop();
        }
    }
}
