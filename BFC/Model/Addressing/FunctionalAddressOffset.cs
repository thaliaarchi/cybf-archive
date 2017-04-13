using CyBF.BFC.Model.Functions;

namespace CyBF.BFC.Model.Addressing
{
    public class FunctionalAddressOffset : AddressOffset
    {
        public FunctionDefinition ReferenceFunction { get; private set; }
        public FunctionDefinition DereferenceFunction { get; private set; }
        public BFObject IndexObject { get; private set; }

        public FunctionalAddressOffset(FunctionDefinition referenceFunction, FunctionDefinition dereferenceFunction, BFObject indexObject)
        {
            this.ReferenceFunction = referenceFunction;
            this.DereferenceFunction = dereferenceFunction;
            this.IndexObject = indexObject;
        }
    }
}
