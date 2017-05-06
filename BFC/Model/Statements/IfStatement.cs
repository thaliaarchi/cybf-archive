using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

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
            */

            BFObject c = compiler.MakeAndMoveToObject(new ByteInstance());
            compiler.Write("[-]");

            BFObject e = compiler.MakeAndMoveToObject(new ByteInstance());
            compiler.Write("[-]");

            // First, copy from conditionObject to c, 
            // using e as the temporary variable since it's not needed yet.
            
            // Move data from conditionObject to both c and e.
            compiler.MoveToObject(conditionObject);
            compiler.Write("[");
            compiler.MoveToObject(c);
            compiler.Write("+");
            compiler.MoveToObject(e);
            compiler.Write("+");
            compiler.MoveToObject(conditionObject);
            compiler.Write("-");
            compiler.Write("]");

            // Move the data from e back to conditionObject.
            compiler.MoveToObject(e);
            compiler.Write("[");
            compiler.MoveToObject(conditionObject);
            compiler.Write("+");
            compiler.MoveToObject(e);
            compiler.Write("-");
            compiler.Write("]");

            // Finally, set e = 1. 
            // If it doesn't change, we will run the elseBody.
            compiler.MoveToObject(e);
            compiler.Write("+");

            /*
            while(c)
            {
                Run conditionalBody code.
                Zero out e, so we don't run the elseBody code.
                Zero out c, since we only want to iterate once.
            }
            */
            compiler.MoveToObject(c);
            compiler.Write("[");

            foreach (Statement statement in this.ConditionalBody)
                statement.Compile(compiler);

            compiler.MoveToObject(e);
            compiler.Write("-");
            compiler.MoveToObject(c);
            compiler.Write("[-]");
            compiler.Write("]");

            /*
            while(e)
            {
                Run elseBody code.
                Zero out e, since we only want to iterate once.
            }
            */
            compiler.MoveToObject(e);
            compiler.Write("[");

            foreach (Statement statement in this.ElseBody)
                statement.Compile(compiler);

            compiler.MoveToObject(e);
            compiler.Write("-");
            compiler.Write("]");
        }
    }
}
