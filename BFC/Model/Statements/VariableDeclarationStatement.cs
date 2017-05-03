using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class VariableDeclarationStatement : Statement
    {
        public Variable Variable { get; private set; }
        public TypeVariable DataType { get; private set; }

        public VariableDeclarationStatement(Token reference, Variable variable, TypeVariable dataType)
            : base(reference)
        {
            this.Variable = variable;
            this.DataType = dataType;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.Variable.Value = compiler.MakeAndMoveToObject(this.DataType.Value, this.Variable.Name);
        }
    }
}
