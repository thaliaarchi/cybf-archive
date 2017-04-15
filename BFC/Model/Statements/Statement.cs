using CyBF.BFC.Compilation;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Statements
{
    public abstract class Statement
    {
        public Token Reference { get; private set; }

        public Statement(Token reference)
        {
            this.Reference = reference;
        }

        public abstract void Compile(BFCompiler compiler);
    }
}
