using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFIL
{
    public class ReferenceTable
    {
        private Dictionary<string, Variable> _variables;
        private List<Variable> _referenceOrder;

        public Variable this[string name]
        {
            get
            {
                return _variables[name];
            }
        }

        public ReferenceTable()
        {
            _variables = new Dictionary<string, Variable>();
            _referenceOrder = new List<Variable>();
        }

        public void Add(Variable variable)
        {
            _variables[variable.Name] = variable;
            _referenceOrder.Add(variable);
        }

        public void RegisterWithoutReference(Variable variable)
        {
            _variables[variable.Name] = variable;
        }

        public bool Contains(string name)
        {
            return _variables.ContainsKey(name);
        }

        public IReadOnlyList<Variable> GetReferenceOrder()
        {
            return _referenceOrder.AsReadOnly();
        }

        public IReadOnlyList<Variable> GetVariables()
        {
            return _variables.Values.ToList().AsReadOnly();
        }
    }
}
