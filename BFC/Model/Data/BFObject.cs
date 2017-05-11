using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Instances;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Data
{
    public class BFObject
    {
        private static int _allocationAutonum = 1;

        private static BFObject _null = null;
        private static BFObject _unallocated = null;

        public BFObject BaseObject { get; private set; }
        public TypeInstance DataType { get; private set; }
        public string AllocationId { get; private set; }
        public IReadOnlyList<AddressOffset> Offsets { get; private set; }

        public static BFObject Null
        {
            get
            {
                if (_null == null)
                {
                    _null = new BFObject();
                    _null.BaseObject = _null;
                    _null.DataType = new ByteInstance();
                    _null.AllocationId = "_NULL_";
                    _null.Offsets = new List<AddressOffset>().AsReadOnly();
                }

                return _null;
            }
        }

        public static BFObject Unallocated
        {
            get
            {
                if (_unallocated == null)
                {
                    _unallocated = new BFObject();
                    _unallocated.BaseObject = _unallocated;
                    _unallocated.DataType = new ByteInstance();
                    _unallocated.AllocationId = "_UNALLOCATED_";
                    _unallocated.Offsets = new List<AddressOffset>().AsReadOnly();
                }

                return _unallocated;
            }
        }

        private BFObject() { }

        public BFObject(TypeInstance dataType)
        {
            this.BaseObject = this;
            this.DataType = dataType;
            this.AllocationId = "obj" + (_allocationAutonum++).ToString();
            this.Offsets = new List<AddressOffset>().AsReadOnly();
        }

        public BFObject(TypeInstance dataType, string allocationIdPrefix)
        {
            this.BaseObject = this;
            this.DataType = dataType;
            this.AllocationId = allocationIdPrefix + "_obj" + (_allocationAutonum++).ToString();
            this.Offsets = new List<AddressOffset>().AsReadOnly();
        }

        public BFObject Derive(TypeInstance dataType, IEnumerable<AddressOffset> additionalOffsets)
        {
            BFObject derivedObject = new BFObject();
            derivedObject.BaseObject = this.BaseObject;
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
