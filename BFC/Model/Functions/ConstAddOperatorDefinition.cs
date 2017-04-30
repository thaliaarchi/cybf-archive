using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Types;

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

        public override void Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<Model.BFObject>(arguments);
            int left = ((ConstInstance)arglist[0].DataType).Value;
            int right = ((ConstInstance)arglist[1].DataType).Value;

            this.ReturnValue.Value = new BFObject(new ConstInstance(left + right));
        }
    }
}
