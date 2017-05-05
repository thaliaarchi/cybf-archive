using System;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Addressing
{
    public class FunctionalAddressOffset : AddressOffset
    {
        public SelectorDefinition Selector { get; private set; }
        public IReadOnlyList<BFObject> IndexArguments { get; private set; }

        public FunctionalAddressOffset(SelectorDefinition selector, IEnumerable<BFObject> indexArguments)
        {
            this.Selector = selector;
            this.IndexArguments = indexArguments.ToList().AsReadOnly();
        }

        public override void Reference(BFCompiler compiler)
        {
            this.Selector.CompileReference(compiler, this.IndexArguments);
        }

        public override void Dereference(BFCompiler compiler)
        {
            this.Selector.CompileDereference(compiler, this.IndexArguments);
        }
    }
}
