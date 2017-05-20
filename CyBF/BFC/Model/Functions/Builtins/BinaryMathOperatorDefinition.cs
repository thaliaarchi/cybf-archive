using System;
using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class BinaryMathOperatorDefinition : FunctionDefinition
    {
        public Func<int, int, int> Operation { get; private set; }

        public BinaryMathOperatorDefinition(string name, Func<int, int, int> operation)
            : base(name,
                new TypeConstraint(ConstDefinition.StaticName),
                new TypeConstraint(ConstDefinition.StaticName))
        {
            this.Operation = operation;
        }

        protected override BFObject OnCompile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<BFObject>(arguments);
            int left = ((ConstInstance)arglist[0].DataType).Value;
            int right = ((ConstInstance)arglist[1].DataType).Value;

            int result = 0;

            try
            {
                result = this.Operation(left, right);
            }
            catch(ArithmeticException ex)
            {
                compiler.RaiseSemanticError(ex.Message);
            }

            return new BFObject(new ConstInstance(result));
        }
    }
}
