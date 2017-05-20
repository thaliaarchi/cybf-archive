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
        public bool IsCompiling { get; private set; }

        public FunctionDefinition(string name, IEnumerable<FunctionParameter> parameters)
            : base(name)
        {
            this.Parameters = parameters.ToList().AsReadOnly();
            this.IsCompiling = false;
        }

        public FunctionDefinition(string name, params TypeConstraint[] constraints)
            : base(name)
        {
            this.Parameters = constraints.Select(c => new FunctionParameter(c)).ToList().AsReadOnly();
            this.IsCompiling = false;
        }

        public override bool Match(string functionName, IEnumerable<TypeInstance> argumentTypes)
        {
            if (this.IsCompiling)
                return false;

            if (this.Name != functionName)
                return false;

            foreach (FunctionParameter parameter in this.Parameters)
                parameter.TypeParameter.Reset();

            return this.Parameters.MatchSequence(argumentTypes, (p, a) => p.TypeParameter.Match(a));
        }
        
        public BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            if (this.IsCompiling)
                compiler.RaiseSemanticError("Recursive functions are not supported.");

            this.IsCompiling = true;
            BFObject result = this.OnCompile(compiler, arguments);
            this.IsCompiling = false;

            return result;
        }

        protected abstract BFObject OnCompile(BFCompiler compiler, IEnumerable<BFObject> arguments);

        protected void ApplyArguments(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            IEnumerable<TypeInstance> argumentTypes = arguments.Select(a => a.DataType);

            foreach (FunctionParameter parameter in this.Parameters)
                parameter.TypeParameter.Reset();
            
            bool argumentsMatch = this.Parameters.MatchSequence(argumentTypes, (p, a) => p.TypeParameter.Match(a));

            if (!argumentsMatch)
                compiler.RaiseSemanticError("Given arguments failed to match with function definition parameters.");

            List<BFObject> argumentList = arguments.ToList();

            for (int i = 0; i < this.Parameters.Count; i++)
                this.Parameters[i].Variable.Value = argumentList[i];
        }
    }
}
