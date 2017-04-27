using CyBF.BFC.Compilation;
using CyBF.Parsing;

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
            if (this.Variable.Value.DataType.Size() > 0)
                compiler.Write(this.Variable.Value.AllocationId);
        }
    }
}
