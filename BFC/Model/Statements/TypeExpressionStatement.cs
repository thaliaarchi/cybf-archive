using CyBF.BFC.Model.Types;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements
{
    public abstract class TypeExpressionStatement : Statement
    {
        public TypeVariable ReturnVariable { get; private set; }

        public TypeExpressionStatement(Token reference) 
            : base(reference)
        {
            this.ReturnVariable = new TypeVariable();
        }
    }
}
