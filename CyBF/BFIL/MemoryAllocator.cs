using System;
using System.Collections.Generic;

namespace CyBF.BFIL
{
    public class MemoryAllocator
    {
        private struct Allocation
        {
            public int Address;
            public int Size;

            public Allocation(int address, int size)
            {
                this.Address = address;
                this.Size = size;
            }
        }

        private LinkedList<Allocation> _allocations
            = new LinkedList<Allocation>();

        private Dictionary<int, LinkedListNode<Allocation>> _allocationsByAddress
            = new Dictionary<int, LinkedListNode<Allocation>>();
        
        public int AllocationMaximum { get; private set; }

        public MemoryAllocator()
        {
            this.AllocationMaximum = 0;
        }

        public int Allocate(int size)
        {
            int address = 0;
            
            if (size <= 0)
                throw new ArgumentOutOfRangeException();

            if (_allocations.First == null)
            {
                _allocationsByAddress[address] = _allocations.AddFirst(new Allocation(address, size));
                this.AllocationMaximum = Math.Max(this.AllocationMaximum, address + size);
                return address;
            }

            if (_allocations.First.Value.Address - address >= size)
            {
                _allocationsByAddress[address] = _allocations.AddFirst(new Allocation(address, size));
                this.AllocationMaximum = Math.Max(this.AllocationMaximum, address + size);
                return address;
            }

            LinkedListNode<Allocation> node = _allocations.First;
            address = node.Value.Address + node.Value.Size;
            bool spaceFound = false;

            while (!spaceFound)
            {
                if (node.Next == null)
                {
                    spaceFound = true;
                }
                else if (node.Next.Value.Address - address >= size)
                {
                    spaceFound = true;
                }
                else
                {
                    node = node.Next;
                    address = node.Value.Address + node.Value.Size;
                }
            }

            _allocationsByAddress[address] = _allocations.AddAfter(node, new Allocation(address, size));
            this.AllocationMaximum = Math.Max(this.AllocationMaximum, address + size);
            return address;
        }

        public void Free(int address)
        {
            LinkedListNode<Allocation> node;

            if (_allocationsByAddress.TryGetValue(address, out node))
            {
                _allocations.Remove(node);
                _allocationsByAddress.Remove(address);
            }
        }
    }
}
