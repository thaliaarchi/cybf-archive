using System;
using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using System.Linq;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class UnaryMathOperatorDefinition : FunctionDefinition
    {
        public Func<int, int> Operation { get; private set; }

        public UnaryMathOperatorDefinition(string name, Func<int, int> operation)
            : base(name,
                new TypeConstraint("Const"))
        {
            this.Operation = operation;
        }

        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            int operand = ((ConstInstance)arguments.Single().DataType).Value;
            int result = this.Operation(operand);

            return new BFObject(new ConstInstance(result));
        }
    }
}
