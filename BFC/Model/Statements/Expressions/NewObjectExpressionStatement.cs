using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class NewObjectExpressionStatement : ExpressionStatement
    {
        public TypeExpressionStatement TypeExpression { get; private set; }

        public NewObjectExpressionStatement(Token reference, TypeExpressionStatement typeExpression) 
            : base(reference)
        {
            this.TypeExpression = typeExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.TypeExpression.Compile(compiler);
            TypeInstance dataType = this.TypeExpression.ReturnVariable.Value;

            if (dataType.Size() == 0)
                this.ReturnVariable.Value = new BFObject(dataType);
            else
                this.ReturnVariable.Value = compiler.AllocateAndMoveToObject(dataType);
        }

        public override bool IsVolatile()
        {
            // Technically, if TypeExpressionStatement evaluates to a TypeInstance of size 0,
            // we could say this is non-volatile. Doesn't matter - kinda pointless to do 
            // a "new" on such a type.

            return true;
        }
    }
}
