using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class VariableDeclarationStatement : Statement
    {
        public Variable Variable { get; private set; }
        public TypeExpressionStatement DataTypeExpression { get; private set; }

        public VariableDeclarationStatement(Token reference, Variable variable, TypeExpressionStatement dataTypeExpression)
            : base(reference)
        {
            this.Variable = variable;
            this.DataTypeExpression = dataTypeExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.DataTypeExpression.Compile(compiler);
            TypeInstance dataType = this.DataTypeExpression.ReturnVariable.Value;

            if (dataType.Size() == 0)
                this.Variable.Value = new BFObject(dataType);
            else
                this.Variable.Value = compiler.AllocateAndMoveToObject(dataType, this.Variable.Name);
        }
    }
}
