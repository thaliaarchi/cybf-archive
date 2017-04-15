using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using CyBF.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFC.Model.Functions
{
    public abstract class FunctionDefinition
    {
        public string Name { get; private set; }
        public IReadOnlyList<FunctionParameter> Parameters { get; private set; }
        public Variable ReturnValue { get; private set; }

        public FunctionDefinition(string name, IEnumerable<FunctionParameter> parameters, Variable returnValue)
        {
            this.Name = name;
            this.Parameters = new List<FunctionParameter>(parameters).AsReadOnly();
            this.ReturnValue = returnValue;
        }

        public bool Match(string functionName, IEnumerable<TypeInstance> argumentTypes)
        {
            if (this.Name != functionName)
                return false;

            return this.Parameters.MatchSequence(argumentTypes, (p, a) => p.TypeParameter.Match(a));
        }
        
        public abstract void Compile(BFCompiler compiler, IEnumerable<BFObject> arguments);

        protected void ApplyArguments(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            if (!this.Match(this.Name, arguments.Select(a => a.DataType)))
                compiler.RaiseSemanticError("Given arguments failed to match with function definition parameters.");

            List<BFObject> argumentList = new List<BFObject>(arguments);

            for (int i = 0; i < this.Parameters.Count; i++)
                this.Parameters[i].Variable.Value = argumentList[i];
        }
    }
}
