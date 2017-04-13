namespace CyBF.BFC.Model.Addressing
{
    public class NumericAddressOffset : AddressOffset
    {
        public int Amount { get; private set; }

        public NumericAddressOffset(int amount)
        {
            this.Amount = amount;
        }
    }
}
