using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements.Commands
{
    public class LoopCommand : Command
    {
        public IReadOnlyList<Command> InnerCommands { get; private set; }

        public LoopCommand(Token reference, IEnumerable<Command> innerCommands) 
            : base(reference)
        {
            this.InnerCommands = innerCommands.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            compiler.Write("[");

            foreach (Command command in this.InnerCommands)
                command.Compile(compiler);

            compiler.Write("]");
        }
    }
}
