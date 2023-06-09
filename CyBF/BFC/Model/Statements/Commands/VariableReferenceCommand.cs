﻿using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

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
            compiler.TracePush(this.Reference);

            if (this.Variable.Value.DataType is StringInstance)
            {
                StringInstance instance = (StringInstance)this.Variable.Value.DataType;
                BFObject bfobject = compiler.GetCachedString(instance);
                compiler.MoveToObject(bfobject);
            }
            else
            {
                if (this.Variable.Value.DataType.Size() == 0)
                    compiler.RaiseSemanticError("Cannot reference zero-sized data type within command block.");

                compiler.MoveToObject(this.Variable.Value);
            }

            compiler.TracePop();
        }
    }
}
