using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using System;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class CastExpressionStatement : ExpressionStatement
    {
        public ExpressionStatement SourceExpression { get; private set; }
        public TypeExpressionStatement TargetTypeExpression { get; private set; }

        public CastExpressionStatement(
            Token reference, 
            ExpressionStatement sourceExpression, 
            TypeExpressionStatement targetTypeExpression) 
            : base(reference)
        {
            this.SourceExpression = sourceExpression;
            this.TargetTypeExpression = targetTypeExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.SourceExpression.Compile(compiler);
            this.TargetTypeExpression.Compile(compiler);

            BFObject sourceObject = this.SourceExpression.ReturnVariable.Value;
            TypeInstance targetType = this.TargetTypeExpression.ReturnVariable.Value;

            if (sourceObject.DataType.Size() != targetType.Size())
            {
                compiler.TracePush(this.Reference);

                compiler.RaiseSemanticError(string.Format(
                    "Unable to cast {0} to {1} due to a size mismatch.",
                    sourceObject.DataType.ToString(), 
                    targetType.ToString()));
            }

            this.ReturnVariable.Value = sourceObject.Derive(targetType);
        }

        public override bool IsVolatile()
        {
            return this.SourceExpression.IsVolatile();
        }
    }
}
