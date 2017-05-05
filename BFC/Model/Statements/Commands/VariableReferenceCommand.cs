using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements.Commands
{
    public class VariableReferenceCommand : Command
    {
        public Variable Variable { get; private set; }

        public VariableReferenceCommand(Token reference, Variable variable) 
            : base(reference)
        {
            this.Variable = variable;
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.MoveToObject(this.Variable.Value);
        }
    }
}
