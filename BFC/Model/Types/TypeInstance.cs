using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public abstract class TypeInstance
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeInstance> TypeArguments { get; private set; }
        public IReadOnlyList<FieldInstance> Fields { get; private set; }

        public TypeInstance(string typeName)
        {
            this.TypeName = typeName;
            this.TypeArguments = new List<TypeInstance>(0).AsReadOnly();
            this.Fields = new List<FieldInstance>(0).AsReadOnly();
        }

        public TypeInstance(string typeName, IEnumerable<TypeInstance> typeArguments, IEnumerable<FieldInstance> fields)
        {
            this.TypeName = typeName;
            this.TypeArguments = typeArguments.ToList().AsReadOnly();
            this.Fields = fields.ToList().AsReadOnly();
        }

        public override string ToString()
        {
            if (this.TypeArguments.Count > 0)
            {
                string arguments = string.Join(" ", this.TypeArguments.Select(a => a.ToString()));
                return "[" + this.TypeName + " " + arguments + "]";
            }
            else
            {
                return "[" + this.TypeName + "]";
            }
        }

        public abstract int Size();
    }
}
