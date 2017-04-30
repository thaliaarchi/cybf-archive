using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Statements;
using System.Linq;

namespace CyBF.BFC.Model.Functions
{
    public class ProcedureDefinition : FunctionDefinition
    {
        public Token Reference { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public ProcedureDefinition(
            Token reference,
            string name,
            Variable returnValue,
            IEnumerable<FunctionParameter> parameters,
            IEnumerable<Statement> body)
            : base(name, returnValue, parameters)
        {
            this.Reference = reference;
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.Reference);
            
            this.ApplyArguments(compiler, arguments);

            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            compiler.TracePop();
        }
    }
}
