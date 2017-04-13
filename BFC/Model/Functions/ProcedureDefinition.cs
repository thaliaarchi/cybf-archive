using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Statements;

namespace CyBF.BFC.Model.Functions
{
    public class ProcedureDefinition : FunctionDefinition
    {
        public Token Reference { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public ProcedureDefinition(
            Token reference, 
            string name, 
            IEnumerable<FunctionParameter> parameters, 
            IEnumerable<Statement> body,
            Variable returnValue) 
            : base(name, parameters, returnValue)
        {
            this.Reference = reference;
            this.Body = new List<Statement>(body).AsReadOnly();
        }

        public override void Compile(BFCompiler compiler, List<BFObject> arguments)
        {
            compiler.TracePush(this.Reference);

            this.ApplyArguments(compiler, arguments);

            foreach (Statement statement in this.Body)
                compiler.CompileStatement(statement);

            compiler.TracePop();
        }
    }
}
