using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class StringStatement : Statement
    {
        public CyBFString String { get; private set; }
        public Variable ReturnValue { get; private set; }

        public StringStatement(Token reference, CyBFString value, Variable returnValue) 
            : base(reference)
        {
            this.String = value;
            this.ReturnValue = returnValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnValue.Value = new BFObject(new StringInstance(this.String), this.ReturnValue.Name);
        }
    }
}
