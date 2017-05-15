using CyBF.BFC.Compilation;

namespace CyBF.BFC.Model.Addressing
{
    public abstract class AddressOffset
    {
        public abstract void Reference(BFCompiler compiler);
        public abstract void Dereference(BFCompiler compiler);
    }
}
