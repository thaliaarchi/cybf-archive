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
        public FunctionParameter SourceParameter { get; private set; }
        public IReadOnlyList<FunctionParameter> IndexParameters { get; private set; }
        public TypeExpressionStatement ReturnTypeExpression { get; private set; }
        public IReadOnlyList<Statement> ReferenceBody { get; private set; }
        public IReadOnlyList<Statement> DereferenceBody { get; private set; }

        public SelectorDefinition(
            Token referenceToken,
            FunctionParameter sourceParameter,
            IEnumerable<FunctionParameter> indexParameters,
            TypeExpressionStatement returnTypeExpression,
            IEnumerable<Statement> referenceBody,
            IEnumerable<Statement> dereferenceBody)
            : base("selector", new FunctionParameter[] { sourceParameter }.Concat(indexParameters))
        {
            this.ReferenceToken = referenceToken;
            this.SourceParameter = sourceParameter;
            this.IndexParameters = indexParameters.ToList().AsReadOnly();
            this.ReturnTypeExpression = returnTypeExpression;
            this.ReferenceBody = referenceBody.ToList().AsReadOnly();
            this.DereferenceBody = dereferenceBody.ToList().AsReadOnly();
        }

        public BFObject Compile(BFCompiler compiler, BFObject sourceArgument, IEnumerable<BFObject> indexArguments)
        {
            IEnumerable<BFObject> functionArguments =
                new BFObject[] { sourceArgument }.Concat(indexArguments);

            return this.Compile(compiler, functionArguments);
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
            
            BFObject result = this.SourceParameter.Variable.Value.Derive(
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
