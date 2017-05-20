using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class AssertFunctionDefinition : FunctionDefinition
    {
        public AssertFunctionDefinition() 
            : base("assert", 
                new TypeConstraint(ConstDefinition.StaticName),
                new TypeConstraint(StringDefinition.StaticName))
        {
        }

        protected override BFObject OnCompile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<BFObject>(arguments);
            int condition = ((ConstInstance)arglist[0].DataType).Value;
            string message = ((StringInstance)arglist[1].DataType).ProcessedString;

            if (condition == 0)
                compiler.RaiseSemanticError(message);

            return new BFObject(new VoidInstance());
        }
    }
}
