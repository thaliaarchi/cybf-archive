using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Functions
{
    public class SelectorDefinition : FunctionDefinition
    {
        public Token ReferenceToken { get; private set; }
        public TypeExpressionStatement ReturnTypeExpression { get; private set; }
        public IReadOnlyList<Statement> ReferenceBody { get; private set; }
        public IReadOnlyList<Statement> DereferenceBody { get; private set; }

        public SelectorDefinition(
            Token referenceToken,
            string name,
            IEnumerable<FunctionParameter> parameters,
            TypeExpressionStatement returnTypeExpression,
            IEnumerable<Statement> referenceBody,
            IEnumerable<Statement> dereferenceBody)
            : base(name, parameters)
        {
            this.ReferenceToken = referenceToken;
            this.ReturnTypeExpression = returnTypeExpression;
            this.ReferenceBody = referenceBody.ToList().AsReadOnly();
            this.DereferenceBody = dereferenceBody.ToList().AsReadOnly();
        }
        
        public override BFObject Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.ReferenceToken);
            
            this.ApplyArguments(compiler, arguments);

            TypeInstance returnType;

            using (compiler.BeginRecursionCheck(this))
            {
                this.ReturnTypeExpression.Compile(compiler);
                returnType = this.ReturnTypeExpression.ReturnVariable.Value;
            }

            if (!arguments.Any())
                compiler.RaiseSemanticError("Empty selector argument list.");

            BFObject sourceObject = arguments.First();

            BFObject result = sourceObject.Derive(
                returnType, new FunctionalAddressOffset(this, arguments));

            compiler.TracePop();

            return result;
        }

        public void CompileReference(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.ReferenceToken);

            this.ApplyArguments(compiler, arguments);

            using (compiler.BeginRecursionCheck(this))
            {
                foreach (Statement statement in this.ReferenceBody)
                    statement.Compile(compiler);
            }

            compiler.TracePop();
        }

        public void CompileDereference(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.ReferenceToken);
            
            this.ApplyArguments(compiler, arguments);

            using (compiler.BeginRecursionCheck(this))
            {
                foreach (Statement statement in this.DereferenceBody)
                    statement.Compile(compiler);
            }
            
            compiler.TracePop();
        }
    }
}
