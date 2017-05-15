using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Statements.Expressions;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements
{
    public class VariableAssignmentStatement : Statement
    {
        public Variable Variable { get; private set; }
        public ExpressionStatement Expression { get; private set; }

        public VariableAssignmentStatement(Token reference, Variable variable, ExpressionStatement expression) 
            : base(reference)
        {
            this.Variable = variable;
            this.Expression = expression;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.Expression.Compile(compiler);
            this.Variable.Value = this.Expression.ReturnVariable.Value;
        }
    }
}
