using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Types.Instances;
using System.Collections.Generic;

namespace CyBF.BFC.Model.Data
{
    public class BFObject
    {
        private static int _allocationAutonum = 1;

        private static BFObject _null = null;
        private static BFObject _unallocated = null;

        public BFObject Parent { get; private set; }
        public AddressOffset Offset { get; private set; }
        public int Depth { get; private set; }

        public TypeInstance DataType { get; private set; }
        public string AllocationId { get; private set; }

        public static BFObject Null
        {
            get
            {
                if (_null == null)
                {
                    _null = new BFObject();

                    _null.Parent = null;
                    _null.Offset = new NumericAddressOffset(0);
                    _null.Depth = 0;

                    _null.DataType = new ByteInstance();
                    _null.AllocationId = "_NULL_";
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

                    _unallocated.Parent = null;
                    _unallocated.Offset = new NumericAddressOffset(0);
                    _unallocated.Depth = 0;

                    _unallocated.DataType = new ByteInstance();
                    _unallocated.AllocationId = "_UNALLOCATED_";
                }

                return _unallocated;
            }
        }

        private BFObject() { }

        public BFObject(TypeInstance dataType)
        {
            this.Parent = null;
            this.Offset = new NumericAddressOffset(0);
            this.Depth = 0;

            this.DataType = dataType;
            this.AllocationId = "obj" + (_allocationAutonum++).ToString();
        }

        public BFObject(TypeInstance dataType, string allocationIdPrefix)
        {
            this.Parent = null;
            this.Offset = new NumericAddressOffset(0);
            this.Depth = 0;

            this.DataType = dataType;
            this.AllocationId = allocationIdPrefix + "_obj" + (_allocationAutonum++).ToString();
        }

        public BFObject Derive(TypeInstance dataType, AddressOffset offset)
        {
            BFObject derivedObject = new BFObject();

            derivedObject.Parent = this;
            derivedObject.Offset = offset;
            derivedObject.Depth = this.Depth + 1;

            derivedObject.DataType = dataType;
            derivedObject.AllocationId = this.AllocationId;
            
            return derivedObject;
        }

        public BFObject Derive(TypeInstance dataType)
        {
            return Derive(dataType, new NumericAddressOffset(0));
        }
    }
}
