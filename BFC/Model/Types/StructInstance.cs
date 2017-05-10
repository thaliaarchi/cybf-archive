using CyBF.BFC.Model.Functions;
using CyBF.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public class StructInstance : TypeInstance
    {
        public Token Reference { get; private set; }

        public StructInstance(
            Token reference, 
            string typeName, 
            IEnumerable<TypeInstance> typeArguments, 
            IEnumerable<FieldInstance> fields,
            DefinitionLibrary<FunctionDefinition> methods) 
            : base(typeName, typeArguments, fields, methods)
        {
            this.Reference = reference;
        }

        public override int Size()
        {
            return this.Fields.Sum(f => f.DataType.Size());
        }
    }
}
