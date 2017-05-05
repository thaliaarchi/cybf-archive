using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class TypeVariableStatement : TypeExpressionStatement
    {
        public TypeVariable InputVariable { get; private set; }

        public TypeVariableStatement(Token reference, TypeVariable variable) 
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
