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
            if (compiler.LastReferencedAllocatedObject == null)
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError("Loop does not have a defined control variable.");
            }

            BFObject beginningObject = compiler.LastReferencedAllocatedObject;

            compiler.Write("[");

            foreach (Command command in this.InnerCommands)
                command.Compile(compiler);

            compiler.Write("]");

            BFObject endingObject = compiler.LastReferencedAllocatedObject;

            if (beginningObject != endingObject)
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError("Loop does not terminate on control variable.");
            }
        }
    }
}
