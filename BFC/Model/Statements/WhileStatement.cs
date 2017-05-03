using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Statements.Commands;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class WhileStatement : Statement
    {
        private VariableReferenceCommand _referenceConditionCommand;

        public Variable Condition { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public WhileStatement(Token reference, Variable condition, IEnumerable<Statement> body) : base(reference)
        {
            this.Condition = condition;
            this.Body = body.ToList().AsReadOnly();

            _referenceConditionCommand = new VariableReferenceCommand(reference, condition);
        }

        public override void Compile(BFCompiler compiler)
        {
            if (!(this.Condition.Value.DataType is ByteInstance))
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError("Condition variable is not a byte.");
            }

            _referenceConditionCommand.Compile(compiler);

            compiler.Write("[");

            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            _referenceConditionCommand.Compile(compiler);

            compiler.Write("]");
        }
    }
}
