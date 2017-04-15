using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFC.Model
{
    public class DefinitionLibrary
    {
        private Dictionary<string, List<FunctionDefinition>> _functions
            = new Dictionary<string, List<FunctionDefinition>>();

        private Dictionary<string, List<TypeDefinition>> _types
            = new Dictionary<string, List<TypeDefinition>>();

        public void DefineFunction(FunctionDefinition definition)
        {
            if (!_functions.ContainsKey(definition.Name))
                _functions[definition.Name] = new List<FunctionDefinition>();

            _functions[definition.Name].Add(definition);
        }

        public void DefineType(TypeDefinition definition)
        {
            if (!_types.ContainsKey(definition.TypeName))
                _types[definition.TypeName] = new List<TypeDefinition>();

            _types[definition.TypeName].Add(definition);
        }

        public List<FunctionDefinition> MatchFunction(string name, IEnumerable<TypeInstance> arguments)
        {
            List<FunctionDefinition> result = new List<FunctionDefinition>();

            if (!_functions.ContainsKey(name))
                return result;

            List<FunctionDefinition> overloads = _functions[name];

            foreach (FunctionDefinition definition in overloads)
            {
                if (definition.Match(name, arguments))
                    result.Add(definition);
            }

            return result;
        }

        public List<TypeDefinition> MatchType(string name, IEnumerable<TypeInstance> arguments)
        {
            List<TypeDefinition> result = new List<TypeDefinition>();

            if (!_types.ContainsKey(name))
                return result;

            List<TypeDefinition> overloads = _types[name];

            foreach (TypeDefinition definition in overloads)
            {
                if (definition.Match(name, arguments))
                    result.Add(definition);
            }

            return result;
        }
    }
}
