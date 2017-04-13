using CyBF.Utility;
using System;
using System.Collections.Generic;

namespace CyBF.BFC.Model.Types
{
    public class TypeConstraint
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeParameter> Parameters { get; private set; }

        public TypeConstraint(string typeName, IEnumerable<TypeParameter> parameters)
        {
            this.TypeName = typeName;
            this.Parameters = new List<TypeParameter>(parameters).AsReadOnly();
        }

        public bool Match(TypeInstance instance)
        {
            return this.Match(instance.TypeName, instance.TypeArguments);
        }

        public bool Match(string typeName, IEnumerable<TypeInstance> arguments)
        {
            List<TypeInstance> arglist = new List<TypeInstance>(arguments);

            if (this.TypeName != typeName)
                return false;

            return this.Parameters.MatchSequence(arguments, (p, a) => p.Match(a));
        }
    }
}
