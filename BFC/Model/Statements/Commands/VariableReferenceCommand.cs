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
            if (this.Variable.Value.DataType.Size() > 0)
            {
                if (compiler.LastReferencedAllocatedObject != null)
                {
                    IReadOnlyList<AddressOffset> undoOffsets = compiler.LastReferencedAllocatedObject.Offsets;

                    foreach (AddressOffset offset in undoOffsets.Reverse())
                        offset.Dereference(compiler);
                }

                compiler.Write(this.Variable.Value.AllocationId + " ");

                foreach (AddressOffset offset in this.Variable.Value.Offsets)
                    offset.Reference(compiler);

                compiler.LastReferencedAllocatedObject = this.Variable.Value;
            }
        }
    }
}
