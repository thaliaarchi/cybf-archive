using CyBF.BFC.Model;
using System.Collections.Generic;

namespace CyBF.BFC.Compilation
{
    public class SymbolTableFrame
    {
        private Dictionary<string, Variable> _variables;

        public SymbolTableFrame Parent { get; private set; }
        
        public Variable this[string name]
        {
            get
            {
                if (_variables.ContainsKey(name))
                    return _variables[name];

                if (this.Parent != null)
                    return this.Parent[name];

                throw new KeyNotFoundException();
            }
            set
            {
                _variables[name] = value;
            }
        }

        public SymbolTableFrame()
        {
            this.Parent = null;
            _variables = new Dictionary<string, Variable>();
        }

        public SymbolTableFrame(SymbolTableFrame parent)
        {
            this.Parent = parent;
            _variables = new Dictionary<string, Variable>();
        }

        public bool Contains(string name)
        {
            if (_variables.ContainsKey(name))
                return true;

            if (this.Parent != null)
                return this.Parent.Contains(name);

            return false;
        }

        public bool TryGetVariable(string name, out Variable variable)
        {
            if (_variables.TryGetValue(name, out variable))
                return true;

            if (this.Parent != null)
                return this.Parent.TryGetVariable(name, out variable);

            variable = null;
            return false;
        }
    }
}
