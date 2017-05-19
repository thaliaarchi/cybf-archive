using System;
using CyBF.BFC.Compilation;

namespace CyBF.BFC.Model.Addressing
{
    public class NumericAddressOffset : AddressOffset
    {
        public int Amount { get; private set; }

        public NumericAddressOffset(int amount)
        {
            this.Amount = amount;
        }

        public override void Reference(BFCompiler compiler)
        {
            char shiftchar = '>';

            if (this.Amount < 0)
                shiftchar = '<';

            compiler.AppendBF(new string(shiftchar, Math.Abs(this.Amount)));
        }

        public override void Dereference(BFCompiler compiler)
        {
            char shiftchar = '<';

            if (this.Amount < 0)
                shiftchar = '>';

            compiler.AppendBF(new string(shiftchar, Math.Abs(this.Amount)));
        }
    }
}
