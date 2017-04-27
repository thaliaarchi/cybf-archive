using System;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements.Commands
{
    public class RepeatCommand : Command
    {
        public IReadOnlyList<Command> InnerCommands { get; private set; }
        public Variable CounterVariable { get; private set; }

        public RepeatCommand(Token reference, IEnumerable<Command> innerCommands, Variable counterVariable) 
            : base(reference)
        {
            this.InnerCommands = innerCommands.ToList().AsReadOnly();
            this.CounterVariable = counterVariable;
        }

        public override void Compile(BFCompiler compiler)
        {
            if (!(this.CounterVariable.Value.DataType is ConstInstance))
                throw new SemanticError("Command repeater counter is not a constant.", this.Reference);

            int value = ((ConstInstance)this.CounterVariable.Value.DataType).Value;

            if (value < 0)
                throw new SemanticError("Command repeater counter is negative.", this.Reference);

            for (int i = 0; i < value; i++)
                foreach (Command command in this.InnerCommands)
                    command.Compile(compiler);
        }
    }
}
