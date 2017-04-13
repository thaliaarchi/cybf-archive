using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public class StructInstance : TypeInstance
    {
        public StructInstance(string typeName, IEnumerable<TypeInstance> typeArguments, IEnumerable<FieldInstance> fields) 
            : base(typeName, typeArguments, fields)
        {
        }

        public override int Size()
        {
            return this.Fields.Sum(f => f.DataType.Size());
        }
    }
}
