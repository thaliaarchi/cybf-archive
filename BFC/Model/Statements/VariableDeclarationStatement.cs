using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Addressing;

namespace CyBF.BFC.Model.Statements
{
    public class VariableDeclarationStatement : Statement
    {
        public Variable Variable { get; private set; }
        public TypeVariable DataType { get; private set; }

        public VariableDeclarationStatement(Token reference, Variable variable, TypeVariable dataType)
            : base(reference)
        {
            this.Variable = variable;
            this.DataType = dataType;
        }

        public override void Compile(BFCompiler compiler)
        {
            BFObject bfobj = new BFObject(this.DataType.Value, this.Variable.Name);
            this.Variable.Value = bfobj;

            int size = this.DataType.Value.Size();

            if (size > 0)
                compiler.Write("@" + bfobj.AllocationId + ":" + size.ToString());
        }
    }
}
