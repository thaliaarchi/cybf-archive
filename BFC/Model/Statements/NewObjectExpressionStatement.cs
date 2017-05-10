using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
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
            BFObject bfobject = compiler.MakeAndMoveToObject(dataType);
            this.ReturnVariable.Value = bfobject;
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
