using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Definitions;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class TupleIndexOperatorDefinition : FunctionDefinition
    {
        public TupleIndexOperatorDefinition() 
            : base("[]", 
                new TypeConstraint(TupleDefinition.StaticName),
                new TypeConstraint(ConstDefinition.StaticName))
        {
        }

        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<BFObject>(arguments);
            IReadOnlyList<BFObject> tupleElements = ((TupleInstance)arglist[0].DataType).Elements;
            int index = ((ConstInstance)arglist[1].DataType).Value;

            if (index < 0 || tupleElements.Count <= index)
            {
                compiler.RaiseSemanticError(string.Format(
                    "Index {0} out of range for tuple of size {1}.",
                    index, tupleElements.Count));
            }

            return tupleElements[index];
        }
    }
}
