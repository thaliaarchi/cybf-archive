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
    }
}
