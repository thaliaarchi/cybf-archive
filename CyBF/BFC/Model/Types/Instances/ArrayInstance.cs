using CyBF.BFC.Model.Types.Definitions;

namespace CyBF.BFC.Model.Types.Instances
{
    public class ArrayInstance : TypeInstance
    {
        public TypeInstance SubType { get; private set; }
        public int Capacity { get; private set; }

        public ArrayInstance(TypeInstance subType, int capacity) 
            : base(ArrayDefinition.StaticName, 
                  new TypeInstance[] { subType }, 
                  new FieldInstance[] { new FieldInstance("capacity", new ConstInstance(capacity), 0) })
        {
            this.SubType = subType;
            this.Capacity = capacity;
        }

        public override int Size()
        {
            return this.SubType.Size() * this.Capacity;
        }
    }
}
