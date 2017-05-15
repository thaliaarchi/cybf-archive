using System;

namespace CyBF.BFC.Compilation
{
    public class RecursionChecker : IDisposable
    {
        private BFCompiler _compiler;
        private Object _item;

        public RecursionChecker(BFCompiler compiler, Object item)
        {
            _compiler = compiler;
            _item = item;
        }

        public void Dispose()
        {
            _compiler.EndRecursionCheck(_item);
        }
    }
}
