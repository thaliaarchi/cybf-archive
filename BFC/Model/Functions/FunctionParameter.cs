using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Functions
{
    public class FunctionParameter
    {
        public Variable Variable { get; private set; }
        public TypeParameter TypeParameter { get; private set; }

        public FunctionParameter(Variable variable, TypeParameter typeParameter)
        {
            this.Variable = variable;
            this.TypeParameter = typeParameter;
        }

        public FunctionParameter(TypeParameter typeParameter)
        {
            this.Variable = new SystemVariable();
            this.TypeParameter = typeParameter;
        }

        public FunctionParameter(TypeConstraint constraint)
        {
            this.Variable = new SystemVariable();
            this.TypeParameter = new ConstrainedTypeParameter(constraint);
        }
    }
}
