using CyBF.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public class TypeConstraint
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeParameter> Parameters { get; private set; }

        public TypeConstraint(string typeName, IEnumerable<TypeParameter> parameters)
        {
            this.TypeName = typeName;
            this.Parameters = parameters.ToList().AsReadOnly();
        }

        public bool Match(TypeInstance instance)
        {
            return this.Match(instance.TypeName, instance.TypeArguments);
        }

        public bool Match(string typeName, IEnumerable<TypeInstance> arguments)
        {
            List<TypeInstance> arglist = arguments.ToList();

            if (this.TypeName != typeName)
                return false;

            return this.Parameters.MatchSequence(arguments, (p, a) => p.Match(a));
        }

        public void Reset()
        {
            foreach (TypeParameter parameter in this.Parameters)
                parameter.Reset();
        }
    }
}
