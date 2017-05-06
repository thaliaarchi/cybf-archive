using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class SizeofFunctionDefinition : FunctionDefinition
    {
        public SizeofFunctionDefinition() 
            : base("sizeof", 
                new TypeParameter())
        {
        }

        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            BFObject bfobj = arguments.Single();
            int size = bfobj.DataType.Size();

            return new BFObject(new ConstInstance(size));
        }
    }
}
