using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

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
                    int numericValue = ((ConstInstance)variable.Value.DataType).Value;

                    if (numericValue < 0 || 255 < numericValue)
                        throw new SemanticError("Invalid variable range for write operation.", this.Reference);

                    writeArguments.Add(numericValue.ToString());
                }
                else if (variable.Value.DataType is StringInstance)
                {
                    string rawString = ((StringInstance)variable.Value.DataType).RawString;
                    writeArguments.Add(rawString);
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
