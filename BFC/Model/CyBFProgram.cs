using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model
{
    public class CyBFProgram
    {
        public IReadOnlyList<TypeDefinition> DataTypes { get; private set; }
        public IReadOnlyList<FunctionDefinition> Functions { get; private set; }
        public IReadOnlyList<Statement> Statements { get; private set; }

        public CyBFProgram(IEnumerable<TypeDefinition> dataTypes, IEnumerable<FunctionDefinition> functions, IEnumerable<Statement> statements)
        {
            this.DataTypes = dataTypes.ToList().AsReadOnly();
            this.Functions = functions.ToList().AsReadOnly();
            this.Statements = statements.ToList().AsReadOnly();
        }

        public string Compile()
        {
            BFCompiler compiler = new BFCompiler(this.DataTypes, this.Functions);

            foreach (Statement statement in this.Statements)
                statement.Compile(compiler);

            return compiler.GetCode();
        }
    }
}
