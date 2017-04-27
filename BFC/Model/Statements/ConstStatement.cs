using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Addressing;

namespace CyBF.BFC.Model.Statements
{
    public class ConstStatement : Statement
    {
        public int NumericValue { get; private set; }
        public Variable ReturnValue { get; private set; }

        public ConstStatement(Token reference, int numericValue, Variable returnValue) 
            : base(reference)
        {
            this.NumericValue = numericValue;
            this.ReturnValue = returnValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            string allocationId = compiler.NewAllocationId(this.ReturnValue.Name);
            this.ReturnValue.Value = new BFObject(new ConstInstance(this.NumericValue), allocationId, new AddressOffset[] { });
        }
    }
}
