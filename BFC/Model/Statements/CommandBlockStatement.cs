using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Statements.Commands;

namespace CyBF.BFC.Model.Statements
{
    public class CommandBlockStatement : Statement
    {
        public IReadOnlyList<Command> Commands { get; private set; }

        public CommandBlockStatement(Token reference, IEnumerable<Command> commands) 
            : base(reference)
        {
            this.Commands = commands.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            foreach (Command cmd in this.Commands)
                cmd.Compile(compiler);
        }
    }
}
