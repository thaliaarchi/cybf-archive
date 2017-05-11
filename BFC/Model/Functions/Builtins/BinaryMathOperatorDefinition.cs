using System;
using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class BinaryMathOperatorDefinition : FunctionDefinition
    {
        public Func<int, int, int> Operation { get; private set; }

        public BinaryMathOperatorDefinition(string name, Func<int, int, int> operation)
            : base(name,
                new TypeConstraint("Const"),
                new TypeConstraint("Const"))
        {
            this.Operation = operation;
        }

        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<BFObject>(arguments);
            int left = ((ConstInstance)arglist[0].DataType).Value;
            int right = ((ConstInstance)arglist[1].DataType).Value;
            int result = this.Operation(left, right);

            return new BFObject(new ConstInstance(result));
        }
    }
}
