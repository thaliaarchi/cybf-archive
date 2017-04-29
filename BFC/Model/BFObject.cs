﻿using CyBF.BFC.Model.Addressing;
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
    }
}
