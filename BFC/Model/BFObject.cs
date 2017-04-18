﻿using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Types;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model
{
    public class BFObject
    {
        public TypeInstance DataType { get; private set; }
        public string AllocationId { get; private set; }
        public IReadOnlyList<AddressOffset> Offsets { get; private set; }

        public BFObject(TypeInstance dataType, string allocationId, IEnumerable<AddressOffset> offsets)
        {
            this.DataType = dataType;
            this.AllocationId = allocationId;
            this.Offsets = offsets.ToList().AsReadOnly();
        }
    }
}
