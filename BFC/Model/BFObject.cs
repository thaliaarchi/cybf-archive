using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Types;
using System.Collections.Generic;

namespace CyBF.BFC.Model
{
    public class BFObject
    {
        public TypeInstance DataType { get; private set; }
        public int AllocationId { get; private set; }
        public IReadOnlyList<AddressOffset> Offsets { get; private set; }

        public BFObject(TypeInstance dataType, int allocationId, IEnumerable<AddressOffset> offsets)
        {
            this.DataType = dataType;
            this.AllocationId = allocationId;
            this.Offsets = new List<AddressOffset>(offsets).AsReadOnly();
        }
    }
}
