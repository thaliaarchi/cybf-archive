using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements.Commands
{
    public class WriteCommand : Command
    {
        public IReadOnlyList<Variable> Variables { get; private set; }

        public WriteCommand(Token reference, IEnumerable<Variable> variables)
            : base(reference)
        {
            this.Variables = variables.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            List<string> writeArguments = new List<string>();

            foreach (Variable variable in this.Variables)
            {
                if (variable.Value.DataType is ConstInstance)
                {
                    int value = ((ConstInstance)variable.Value.DataType).Value;

                    if (value < 0 || 255 < value)
                        throw new SemanticError("Invalid variable range for write operation.", this.Reference);

                    writeArguments.Add(value.ToString());
                }
                else if (variable.Value.DataType is StringInstance)
                {
                    string value = ((StringInstance)variable.Value.DataType).String.RawValue;
                    writeArguments.Add(value);
                }
                else
                {
                    throw new SemanticError("Invalid variable type for write operation.", this.Reference);
                }
            }

            compiler.Write("#(");
            compiler.Write(string.Join(", ", writeArguments));
            compiler.Write(")");
        }
    }
}
