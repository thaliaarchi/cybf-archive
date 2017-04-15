using CyBF.BFC.Compilation;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model.Types
{
    public abstract class TypeDefinition
    {
        public TypeConstraint Constraint { get; private set; }
        public string TypeName { get { return this.Constraint.TypeName; } }
        public IReadOnlyList<Variable> Parameters { get; private set; }
        
        public TypeDefinition(TypeConstraint constraint, IEnumerable<Variable> parameters)
        {
            this.Constraint = constraint;
            this.Parameters = new List<Variable>(parameters).AsReadOnly();
        }

        public bool Match(TypeInstance instance)
        {
            this.Constraint.Reset();
            return this.Constraint.Match(instance);
        }

        public bool Match(string typeName, IEnumerable<TypeInstance> arguments)
        {
            this.Constraint.Reset();
            return this.Constraint.Match(typeName, arguments);
        }

        public abstract TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments);

        protected void ApplyArguments(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            if (!this.Match(this.Constraint.TypeName, typeArguments))
                compiler.RaiseSemanticError("Type arguments do not match with type definition constraint.");

            List<BFObject> valueArgumentList = new List<BFObject>(valueArguments);

            if (valueArgumentList.Count != this.Parameters.Count)
                compiler.RaiseSemanticError("Value argument count does not match type definition parameter count.");

            if (valueArgumentList.Any(obj => !(obj.DataType is ConstInstance)))
                compiler.RaiseSemanticError("Non constant value argument to type definition found.");

            for (int i = 0; i < this.Parameters.Count; i++)
                this.Parameters[i].Value = valueArgumentList[i];
        }
    }
}
