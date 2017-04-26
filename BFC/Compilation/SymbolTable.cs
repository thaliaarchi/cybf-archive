using CyBF.BFC.Model;
using System;

namespace CyBF.BFC.Compilation
{
    public class SymbolTable
    {
        private SymbolTableFrame _frames;

        public Variable this[string name]
        {
            get { return _frames[name]; }
            set { _frames[name] = value; }
        }

        public SymbolTable()
        {
            _frames = new SymbolTableFrame();
        }

        public void Push()
        {
            _frames = new SymbolTableFrame(_frames);
        }

        public void Pop()
        {
            if (_frames.Parent == null)
                throw new InvalidOperationException("Attempted to pop top-level SymbolTableFrame.");

            _frames = _frames.Parent;
        }

        public bool Contains(string name)
        {
            return _frames.Contains(name);
        }

        public bool TryGetVariable(string name, out Variable variable)
        {
            return _frames.TryGetVariable(name, out variable);
        }
    }
}
