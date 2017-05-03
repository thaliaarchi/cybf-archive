using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Types;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model
{
    public class BFObject
    {
        private static int _allocationAutonum = 1;

        public TypeInstance DataType { get; private set; }
        public string AllocationId { get; private set; }
        public IReadOnlyList<AddressOffset> Offsets { get; private set; }

        private BFObject() { }

        public BFObject(TypeInstance dataType)
        {
            this.DataType = dataType;
            this.AllocationId = "obj" + (_allocationAutonum++).ToString();
            this.Offsets = new List<AddressOffset>().AsReadOnly();
        }

        public BFObject(TypeInstance dataType, string allocationIdPrefix)
        {
            this.DataType = dataType;
            this.AllocationId = allocationIdPrefix + "_obj" + (_allocationAutonum++).ToString();
            this.Offsets = new List<AddressOffset>().AsReadOnly();
        }

        public BFObject Derive(TypeInstance dataType, IEnumerable<AddressOffset> additionalOffsets)
        {
            BFObject derivedObject = new BFObject();
            derivedObject.DataType = dataType;
            derivedObject.AllocationId = this.AllocationId;
            derivedObject.Offsets = this.Offsets.Concat(additionalOffsets).ToList().AsReadOnly();
            return derivedObject;
        }

        public BFObject Derive(TypeInstance dataType, params AddressOffset[] additionalOffsets)
        {
            return Derive(dataType, (IEnumerable<AddressOffset>)additionalOffsets);
        }

        public void ApplyOffsets(BFCompiler compiler)
        {
            foreach (AddressOffset offset in this.Offsets)
                offset.Reference(compiler);
        }

        public void UndoOffsets(BFCompiler compiler)
        {
            foreach (AddressOffset offset in this.Offsets.Reverse())
                offset.Dereference(compiler);
        }
    }
}
