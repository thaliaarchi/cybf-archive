using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Functions
{
    public class ConstAddOperatorDefinition : FunctionDefinition
    {
        public ConstAddOperatorDefinition()
            : base("+", 
                new TypeConstraint("Const"),
                new TypeConstraint("Const"))
        {
        }

        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<BFObject>(arguments);
            int left = ((ConstInstance)arglist[0].DataType).Value;
            int right = ((ConstInstance)arglist[1].DataType).Value;

            return new BFObject(new ConstInstance(left + right));
        }
    }
}
