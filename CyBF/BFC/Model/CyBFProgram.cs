using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Definitions;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Model
{
    public class CyBFProgram
    {
        public DefinitionLibrary<TypeDefinition> TypeLibrary { get; private set; }
        public DefinitionLibrary<FunctionDefinition> FunctionLibrary { get; private set; }
        public IReadOnlyList<Statement> Statements { get; private set; }

        public CyBFProgram(
            DefinitionLibrary<TypeDefinition> typeLibrary, 
            DefinitionLibrary<FunctionDefinition> functionLibrary, 
            IEnumerable<Statement> statements)
        {
            this.TypeLibrary = typeLibrary;
            this.FunctionLibrary = functionLibrary;
            this.Statements = statements.ToList().AsReadOnly();
        }

        public string Compile()
        {
            BFCompiler compiler = new BFCompiler(this.TypeLibrary, this.FunctionLibrary);

            foreach (Statement statement in this.Statements)
                statement.Compile(compiler);

            return compiler.GetCode();
        }
    }
}
