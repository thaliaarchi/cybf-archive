using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Instances;
using System.Collections.Generic;

namespace CyBF.BFC.Model
{
    public abstract class Definition
    {
        public string Name { get; private set; }

        public Definition(string name)
        {
            this.Name = name;
        }

        public abstract bool Match(string name, IEnumerable<TypeInstance> arguments);
    }
}
