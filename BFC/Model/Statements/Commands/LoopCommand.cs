using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;

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
            if (compiler.CurrentAllocatedObject == null)
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError("Loop does not have a defined control variable.");
            }

            BFObject beginningObject = compiler.CurrentAllocatedObject;

            compiler.Write("[");

            foreach (Command command in this.InnerCommands)
                command.Compile(compiler);

            compiler.Write("]");

            BFObject endingObject = compiler.CurrentAllocatedObject;

            if (beginningObject != endingObject)
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError("Loop does not terminate on control variable.");
            }
        }
    }
}
