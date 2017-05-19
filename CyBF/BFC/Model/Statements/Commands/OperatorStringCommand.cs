using CyBF.BFC.Compilation;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements.Commands
{
    public class OperatorStringCommand : Command
    {
        public string Operators { get; private set; }

        public OperatorStringCommand(Token reference, string operators) 
            : base(reference)
        {
            this.Operators = operators;
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.AppendBF(this.Operators);
        }
    }
}
