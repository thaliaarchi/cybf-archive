using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class TupleExpressionStatement : ExpressionStatement
    {
        public IReadOnlyList<ExpressionStatement> Arguments { get; private set; }

        public TupleExpressionStatement(Token reference, IEnumerable<ExpressionStatement> arguments) 
            : base(reference)
        {
            this.Arguments = arguments.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            foreach (ExpressionStatement argument in this.Arguments)
                argument.Compile(compiler);

            IEnumerable<BFObject> elements = this.Arguments.Select(arg => arg.ReturnVariable.Value);
            this.ReturnVariable.Value = new BFObject(new TupleInstance(elements));
        }

        public override bool IsVolatile()
        {
            return this.Arguments.Any(arg => arg.IsVolatile());
        }
    }
}
