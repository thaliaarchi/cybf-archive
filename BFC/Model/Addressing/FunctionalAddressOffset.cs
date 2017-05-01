using System;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Addressing
{
    public class FunctionalAddressOffset : AddressOffset
    {
        public FunctionDefinition ReferenceFunction { get; private set; }
        public FunctionDefinition DereferenceFunction { get; private set; }
        public IReadOnlyList<BFObject> IndexObjects { get; private set; }

        public FunctionalAddressOffset(FunctionDefinition referenceFunction, FunctionDefinition dereferenceFunction, IEnumerable<BFObject> indexObjects)
        {
            this.ReferenceFunction = referenceFunction;
            this.DereferenceFunction = dereferenceFunction;
            this.IndexObjects = indexObjects.ToList().AsReadOnly();
        }

        public override void Reference(BFCompiler compiler)
        {
            this.ReferenceFunction.Compile(compiler, this.IndexObjects);
        }

        public override void Dereference(BFCompiler compiler)
        {
            this.DereferenceFunction.Compile(compiler, this.IndexObjects);
        }
    }
}
