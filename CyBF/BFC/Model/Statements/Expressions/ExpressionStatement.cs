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

        /*
            Returns whether or not this expressions returns BFObjects at distinct 
            addresses on consecutive evaluations. Expressions aren't considered volatile
            just because they return distinct BFObjects, if those objects always reference the same memory
            (e.g., like how type casts create derived BFObjects that point to the originals).

            Used by loops to determine whether to allocate a separate control variable,
            or to simply use the return value of their condition expressions as the control, 
            since loops cannot change their control variables.
        */
        public abstract bool IsVolatile();
    }
}
