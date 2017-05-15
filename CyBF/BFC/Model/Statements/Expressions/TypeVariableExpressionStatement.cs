using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class TypeVariableExpressionStatement : TypeExpressionStatement
    {
        public TypeVariable InputVariable { get; private set; }

        public TypeVariableExpressionStatement(Token reference, TypeVariable variable) 
            : base(reference)
        {
            this.InputVariable = variable;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = this.InputVariable.Value;
        }
    }
}
