namespace CyBF.BFC.Model.Types
{
    public class ConstrainedTypeParameter : TypeParameter
    {
        public TypeConstraint Constraint { get; private set; }

        public ConstrainedTypeParameter(TypeConstraint constraint)
        {
            this.Constraint = constraint;
        }

        public ConstrainedTypeParameter(TypeVariable typeVariable, TypeConstraint constraint) 
            : base(typeVariable)
        {
            this.Constraint = constraint;
        }

        public override bool Match(TypeInstance instance)
        {
            if (!this.Constraint.Match(instance))
                return false;

            return base.Match(instance);
        }

        public override void Reset()
        {
            this.Constraint.Reset();
            base.Reset();
        }
    }
}
