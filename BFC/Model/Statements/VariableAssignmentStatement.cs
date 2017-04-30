using CyBF.BFC.Compilation;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements
{
    public class VariableAssignmentStatement : Statement
    {
        public Variable Destination { get; private set; }
        public Variable Source { get; private set; }

        public VariableAssignmentStatement(Token reference, Variable destination, Variable source) 
            : base(reference)
        {
            this.Destination = destination;
            this.Source = source;
        }

        public override void Compile(BFCompiler compiler)
        {
            Destination.Value = Source.Value;
        }
    }
}
