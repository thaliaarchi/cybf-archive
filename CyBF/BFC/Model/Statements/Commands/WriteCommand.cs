using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Statements.Expressions;

namespace CyBF.BFC.Model.Statements.Commands
{
    public class WriteCommand : Command
    {
        public IReadOnlyList<ExpressionStatement> DataItems { get; private set; }

        public WriteCommand(Token reference, IEnumerable<ExpressionStatement> dataItems)
            : base(reference)
        {
            this.DataItems = dataItems.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            List<byte> data = new List<byte>();

            foreach (ExpressionStatement dataItem in this.DataItems)
            {
                dataItem.Compile(compiler);
                TypeInstance dataType = dataItem.ReturnVariable.Value.DataType;

                if (dataType is ConstInstance)
                {
                    int numericValue = ((ConstInstance)dataType).Value;

                    if (numericValue < 0 || 255 < numericValue)
                        throw new SemanticError("Invalid variable range for write operation.", this.Reference);

                    data.Add((byte)numericValue);
                }
                else if (dataType is CharacterInstance)
                {
                    byte ordinal = ((CharacterInstance)dataType).Ordinal;
                    data.Add(ordinal);
                }
                else if (dataType is StringInstance)
                {
                    byte[] asciiBytes = ((StringInstance)dataType).AsciiBytes;
                    data.Add(0);
                    data.AddRange(asciiBytes);
                    data.Add(0);
                }
                else
                {
                    throw new SemanticError("Invalid data item type for write operation.", this.Reference);
                }
            }

            compiler.WriteData(data);
        }
    }
}
