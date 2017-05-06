using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.Utility;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Functions
{
    public abstract class FunctionDefinition
    {
        public string Name { get; private set; }
        public IReadOnlyList<FunctionParameter> Parameters { get; private set; }

        public FunctionDefinition(string name, IEnumerable<FunctionParameter> parameters)
        {
            this.Name = name;
            this.Parameters = parameters.ToList().AsReadOnly();
        }

        public FunctionDefinition(string name, params TypeParameter[] typeParameters)
        {
            this.Name = name;
            this.Parameters = typeParameters.Select(tp => new FunctionParameter(tp)).ToList().AsReadOnly();
        }

        public FunctionDefinition(string name, params TypeConstraint[] constraints)
        {
            this.Name = name;
            this.Parameters = constraints.Select(c => new FunctionParameter(c)).ToList().AsReadOnly();
        }

        public bool Match(string functionName, IEnumerable<TypeInstance> argumentTypes)
        {
            if (this.Name != functionName)
                return false;

            foreach (FunctionParameter parameter in this.Parameters)
                parameter.TypeParameter.Reset();

            return this.Parameters.MatchSequence(argumentTypes, (p, a) => p.TypeParameter.Match(a));
        }
        
        public abstract BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments);

        protected void ApplyArguments(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            if (!this.Match(this.Name, arguments.Select(a => a.DataType)))
                compiler.RaiseSemanticError("Given arguments failed to match with function definition parameters.");

            List<BFObject> argumentList = arguments.ToList();

            for (int i = 0; i < this.Parameters.Count; i++)
                this.Parameters[i].Variable.Value = argumentList[i];
        }
    }
}
