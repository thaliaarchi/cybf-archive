using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Instances;
using CyBF.Utility;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Functions
{
    public abstract class FunctionDefinition : Definition
    {
        public IReadOnlyList<FunctionParameter> Parameters { get; private set; }

        public FunctionDefinition(string name, IEnumerable<FunctionParameter> parameters)
            : base(name)
        {
            this.Parameters = parameters.ToList().AsReadOnly();
        }

        public FunctionDefinition(string name, params TypeConstraint[] constraints)
            : base(name)
        {
            this.Parameters = constraints.Select(c => new FunctionParameter(c)).ToList().AsReadOnly();
        }

        public override bool Match(string functionName, IEnumerable<TypeInstance> argumentTypes)
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
