using System.Collections.Generic;

namespace CyBF.BFC.Model.Types
{
    public abstract class TypeInstance
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeInstance> TypeArguments { get; private set; }
        public IReadOnlyList<FieldInstance> Fields { get; private set; }

        public TypeInstance(string typeName, IEnumerable<TypeInstance> typeArguments, IEnumerable<FieldInstance> fields)
        {
            this.TypeName = typeName;
            this.TypeArguments = new List<TypeInstance>(typeArguments).AsReadOnly();
            this.Fields = new List<FieldInstance>(fields).AsReadOnly();
        }

        public abstract int Size();
    }
}
