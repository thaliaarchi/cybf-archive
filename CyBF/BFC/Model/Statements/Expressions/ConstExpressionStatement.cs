using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class ConstExpressionStatement : ExpressionStatement
    {
        public int NumericValue { get; private set; }

        public ConstExpressionStatement(Token reference) 
            : base(reference)
        {
            this.NumericValue = reference.NumericValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = new BFObject(new ConstInstance(this.NumericValue));
        }

        public override bool IsVolatile()
        {
            return false;
        }
    }
}
