using System;
using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using System.Linq;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class UnaryMathOperatorDefinition : FunctionDefinition
    {
        public Func<int, int> Operation { get; private set; }

        public UnaryMathOperatorDefinition(string name, Func<int, int> operation)
            : base(name,
                new TypeConstraint(ConstDefinition.StaticName))
        {
            this.Operation = operation;
        }

        protected override BFObject OnCompile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            int operand = ((ConstInstance)arguments.Single().DataType).Value;
            int result = 0;
            
            try
            {
                result = this.Operation(operand);
            }
            catch(ArithmeticException ex)
            {
                compiler.RaiseSemanticError(ex.Message);
            }

            return new BFObject(new ConstInstance(result));
        }
    }
}
