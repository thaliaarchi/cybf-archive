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
            string allocationId = compiler.NewAllocationId(this.Variable.Name);
            this.Variable.Value = new BFObject(this.DataType.Value, allocationId, new AddressOffset[] { });

            int size = this.DataType.Value.Size();

            if (size > 0)
                compiler.WriteLine("@" + allocationId + ":" + size.ToString());
        }
    }
}
