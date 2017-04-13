namespace CyBF.BFC.Model.Types
{
    public class TypeParameter
    {
        public TypeVariable TypeVariable { get; private set; }

        public TypeParameter(TypeVariable typeVariable)
        {
            this.TypeVariable = typeVariable;
        }

        public virtual bool Match(TypeInstance instance)
        {
            this.TypeVariable.Value = instance;
            return true;
        }
    }
}
