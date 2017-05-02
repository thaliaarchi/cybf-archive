namespace CyBF.BFC.Model.Types
{
    public class TypeParameter
    {
        public TypeVariable TypeVariable { get; private set; }

        public TypeParameter()
        {
            this.TypeVariable = new TypeVariable();
        }

        public TypeParameter(TypeVariable typeVariable)
        {
            this.TypeVariable = typeVariable;
        }

        public virtual bool Match(TypeInstance instance)
        {
            if (this.TypeVariable.Value == null)
            {
                this.TypeVariable.Value = instance;
                return true;
            }
            else
            {
                return this.TypeVariable.Value.Matches(instance);
            }
        }

        public virtual void Reset()
        {
            this.TypeVariable.Value = null;
        }
    }
}
