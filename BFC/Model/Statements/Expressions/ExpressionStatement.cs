using CyBF.Parsing;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public abstract class ExpressionStatement : Statement
    {
        public Variable ReturnVariable { get; private set; }

        public ExpressionStatement(Token reference) 
            : base(reference)
        {
            this.ReturnVariable = new SystemVariable();
        }

        public abstract bool IsVolatile();
    }
}
