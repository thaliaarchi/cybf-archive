using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Definitions;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Types.Instances
{
    public class TupleInstance : TypeInstance
    {
        public IReadOnlyList<BFObject> Elements { get; private set; }

        public TupleInstance(IEnumerable<BFObject> elements)
            : base(TupleDefinition.StaticName,
                new TypeInstance[] { },
                new FieldInstance[] { new FieldInstance("size", new ConstInstance(elements.Count()), 0) })
        {
            this.Elements = elements.ToList().AsReadOnly();
        }

        public override int Size()
        {
            return 0;
        }
    }
}
