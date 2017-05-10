using CyBF.BFC.Model.Types;
using System.Collections.Generic;

namespace CyBF.BFC.Model
{
    public class DefinitionLibrary<T>
        where T : Definition
    {
        private Dictionary<string, List<T>> _definitions
            = new Dictionary<string, List<T>>();
        
        public DefinitionLibrary(IEnumerable<T> definitions)
        {
            foreach (T def in definitions)
            {
                if (!_definitions.ContainsKey(def.Name))
                    _definitions[def.Name] = new List<T>();

                _definitions[def.Name].Add(def);
            }
        }

        public DefinitionLibrary(params T[] definitions)
            : this((IEnumerable<T>)definitions)
        {
        }

        public List<T> Match(string name, IEnumerable<TypeInstance> arguments)
        {
            List<T> result = new List<T>();

            if (!_definitions.ContainsKey(name))
                return result;

            List<T> overloads = _definitions[name];

            foreach (T definition in overloads)
            {
                if (definition.Match(name, arguments))
                    result.Add(definition);
            }

            return result;
        }
    }
}
