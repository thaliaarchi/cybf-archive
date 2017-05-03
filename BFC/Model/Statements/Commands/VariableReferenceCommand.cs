using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Addressing;
using CyBF.Parsing;
using System.Collections.Generic;
using System;
using System.Linq;

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
