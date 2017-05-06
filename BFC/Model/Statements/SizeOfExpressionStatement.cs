using System;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class SizeOfExpressionStatement : ExpressionStatement
    {
        public ExpressionStatement Expression { get; private set; }
        public TypeExpressionStatement TypeExpression { get; private set; }
        
        public SizeOfExpressionStatement(Token reference, ExpressionStatement expression) 
            : base(reference)
        {
            this.Expression = expression;
            this.TypeExpression = null;
        }

        public SizeOfExpressionStatement(Token reference, TypeExpressionStatement typeExpression)
            : base(reference)
        {
            this.Expression = null;
            this.TypeExpression = typeExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            TypeInstance datatype;

            if (this.Expression != null)
            {
                this.Expression.Compile(compiler);
                datatype = this.Expression.ReturnVariable.Value.DataType;
            }
            else if (this.TypeExpression != null)
            {
                this.TypeExpression.Compile(compiler);
                datatype = this.TypeExpression.ReturnVariable.Value;
            }
            else
            {
                throw new InvalidOperationException();
            }

            this.ReturnVariable.Value = new BFObject(new ConstInstance(datatype.Size()));
        }
    }
}
