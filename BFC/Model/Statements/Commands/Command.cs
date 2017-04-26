using CyBF.BFC.Compilation;

namespace CyBF.BFC.Model.Statements.Commands
{
    public abstract class Command
    {
        public abstract void Compile(BFCompiler compiler);
    }
}
