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

                return;
            }

            if (!(conditionObject.DataType is ByteInstance))
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError(string.Format(
                    "Condition expression evaluates to '{0}'. Must evaluate to Const or Byte.",
                    conditionObject.DataType.ToString()));
            }

            /*
            // We want:
            //  c = copy of conditionObject (to be zeroed out), 
            //  e = flag controlling whether to run elseBody code
            //      (only if condition is already zero).
            //      Also used as a temp variable for copying conditionObject to c.
            */

            BFObject c = compiler.AllocateAndMoveToObject(new ByteInstance());
            compiler.AppendBF("[-]");

            BFObject e = compiler.AllocateAndMoveToObject(new ByteInstance());
            compiler.AppendBF("[-]");

            // First, copy from conditionObject to c, 
            // using e as the temporary variable since it's not needed yet.
            
            // Move data from conditionObject to both c and e.
            compiler.MoveToObject(conditionObject);
            compiler.BeginCheckedLoop();
            compiler.MoveToObject(c);
            compiler.AppendBF("+");
            compiler.MoveToObject(e);
            compiler.AppendBF("+");
            compiler.MoveToObject(conditionObject);
            compiler.AppendBF("-");
            compiler.EndCheckedLoop();

            // Move the data from e back to conditionObject.
            compiler.MoveToObject(e);
            compiler.BeginCheckedLoop();
            compiler.MoveToObject(conditionObject);
            compiler.AppendBF("+");
            compiler.MoveToObject(e);
            compiler.AppendBF("-");
            compiler.EndCheckedLoop();

            // Leave 'e' at 0 if there is no else body.
            if (this.ElseBody.Count > 0)
            {
                // Finally, set e = 1. 
                // If it doesn't change after this, we will run the elseBody.
                compiler.MoveToObject(e);
                compiler.AppendBF("+");
            }

            /*
            while(c)
            {
                Run conditionalBody code.
                If there is elseBody code, zero out e so we don't run it.
                Zero out c, since we only want to iterate once.
            }
            */
            compiler.MoveToObject(c);
            compiler.BeginCheckedLoop();

            foreach (Statement statement in this.ConditionalBody)
                statement.Compile(compiler);

            if (this.ElseBody.Count > 0)
            {
                compiler.MoveToObject(e);
                compiler.AppendBF("-");
            }

            compiler.MoveToObject(c);
            compiler.AppendBF("[-]");
            compiler.EndCheckedLoop();

            // Only compile an else body if there is one.
            if (this.ElseBody.Count > 0)
            {
                /*
                while(e)
                {
                    Run elseBody code.
                    Zero out e, since we only want to iterate once.
                }
                */
                compiler.MoveToObject(e);
                compiler.BeginCheckedLoop();

                foreach (Statement statement in this.ElseBody)
                    statement.Compile(compiler);

                compiler.MoveToObject(e);
                compiler.AppendBF("-");
                compiler.EndCheckedLoop();
            }
        }
    }
}
