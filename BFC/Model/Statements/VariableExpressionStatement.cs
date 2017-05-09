using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using System;

namespace CyBF.BFC.Model.Statements
{
    public class VariableExpressionStatement : ExpressionStatement
    {
        public Variable InputVariable { get; private set; }

        public VariableExpressionStatement(Token reference, Variable variable) 
            : base(reference)
        {
            this.InputVariable = variable;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = this.InputVariable.Value;
        }

        public override bool IsVolatile()
        {
            return false;
        }
    }
}
