using CyBF.BFC.Compilation;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements.Commands
{
    public abstract class Command
    {
        public Token Reference { get; private set; }

        public Command(Token reference)
        {
            this.Reference = reference;
        }

        public abstract void Compile(BFCompiler compiler);
    }
}
