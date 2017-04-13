using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;

namespace CyBF.BFC.Model.Types
{
    public class ArrayDefinition : TypeDefinition
    {
        public const string StaticName = "Array";

        public ArrayDefinition()
            : base(
                new TypeConstraint(StaticName, new TypeParameter[] { new TypeParameter(new TypeVariable()) }),
                new Variable[] { new Variable() })
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);

            TypeInstance subType = typeArguments.Single();
            int capacity = ((ConstInstance)valueArguments.Single().DataType).Value;

            if (capacity < 0)
                compiler.RaiseSemanticError("Array given a negative capacity.");

            return new ArrayInstance(subType, capacity);
        }
    }
}
