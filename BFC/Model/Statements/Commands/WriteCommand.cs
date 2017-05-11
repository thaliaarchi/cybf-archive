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
            List<string> writeArguments = new List<string>();

            foreach (ExpressionStatement dataItem in this.DataItems)
            {
                dataItem.Compile(compiler);
                TypeInstance dataType = dataItem.ReturnVariable.Value.DataType;

                if (dataType is ConstInstance)
                {
                    int numericValue = ((ConstInstance)dataType).Value;

                    if (numericValue < 0 || 255 < numericValue)
                        throw new SemanticError("Invalid variable range for write operation.", this.Reference);

                    writeArguments.Add(numericValue.ToString());
                }
                else if (dataType is StringInstance)
                {
                    string rawString = ((StringInstance)dataType).RawString;
                    writeArguments.Add(rawString);
                }
                else
                {
                    throw new SemanticError("Invalid data item type for write operation.", this.Reference);
                }
            }

            compiler.Write("#(");
            compiler.Write(string.Join(", ", writeArguments));
            compiler.Write(")");
        }
    }
}
