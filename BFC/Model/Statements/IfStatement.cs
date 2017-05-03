using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class IfStatement : Statement
    {
        public Variable Condition { get; private set; }
        public IReadOnlyList<Statement> ConditionalBody { get; private set; }
        public IReadOnlyList<Statement> ElseBody { get; private set; }

        public IfStatement(Token reference, Variable condition, IEnumerable<Statement> conditionalBody, IEnumerable<Statement> elseBody) 
            : base(reference)
        {
            this.Condition = condition;
            this.ConditionalBody = conditionalBody.ToList().AsReadOnly();
            this.ElseBody = elseBody.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            if (!(this.Condition.Value.DataType is ByteInstance))
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError("Condition variable is not a byte.");
            }

            // We want t0 = conditional, t1 = copy of conditional, t2 = 1.

            BFObject t0 = this.Condition.Value;

            BFObject t1 = compiler.MakeAndMoveToObject(new ByteInstance());
            compiler.Write("[-]");

            BFObject t2 = compiler.MakeAndMoveToObject(new ByteInstance());
            compiler.Write("[-]");
            
            // Move the data from t0 to t1 and t2. This clears t0.
            compiler.MoveToObject(t0);
            compiler.Write("[");
            compiler.MoveToObject(t1);
            compiler.Write("+");
            compiler.MoveToObject(t2);
            compiler.Write("+");
            compiler.MoveToObject(t0);
            compiler.Write("-");
            compiler.Write("]");

            // Move the data from t2 back to t0. This clears t2.
            compiler.MoveToObject(t2);
            compiler.Write("[");
            compiler.MoveToObject(t0);
            compiler.Write("+");
            compiler.MoveToObject(t2);
            compiler.Write("-");
            compiler.Write("]");

            // Finally, increment t2. Now we have the temp object set how we want.
            compiler.MoveToObject(t2);
            compiler.Write("+");

            // while t1 ( conditional body, zero both t1 and t2 )
            compiler.MoveToObject(t1);
            compiler.Write("[");

            foreach (Statement statement in this.ConditionalBody)
                statement.Compile(compiler);

            compiler.MoveToObject(t1);
            compiler.Write("[-]");
            compiler.MoveToObject(t2);
            compiler.Write("-");
            compiler.Write("]");

            // while t2 ( else body, zero t2 )
            compiler.MoveToObject(t2);
            compiler.Write("[");

            foreach (Statement statement in this.ElseBody)
                statement.Compile(compiler);

            compiler.MoveToObject(t2);
            compiler.Write("-");
            compiler.Write("]");
        }
    }
}
