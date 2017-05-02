using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using CyBF.BFC.Model.Addressing;

namespace CyBF.BFC.Model.Functions
{
    public class SelectorDefinition : FunctionDefinition
    {
        public Token ReferenceToken { get; private set; }
        public FunctionParameter Subject { get; private set; }
        public IReadOnlyList<FunctionParameter> IndexParameters { get; private set; }
        public TypeVariable ReturnType { get; private set; }
        public IReadOnlyList<Statement> ReturnTypeConstructor { get; private set; }
        public IReadOnlyList<Statement> ReferenceBody { get; private set; }
        public IReadOnlyList<Statement> DereferenceBody { get; private set; }

        public SelectorDefinition(
            Token referenceToken,
            Variable returnValue,
            FunctionParameter subject,
            IEnumerable<FunctionParameter> indexParameters,
            TypeVariable returnType,
            IEnumerable<Statement> returnTypeConstructor,
            IEnumerable<Statement> referenceBody,
            IEnumerable<Statement> dereferenceBody)
            : base("selector", returnValue, new FunctionParameter[] { subject }.Concat(indexParameters))
        {
            this.ReferenceToken = referenceToken;
            this.Subject = subject;
            this.IndexParameters = indexParameters.ToList().AsReadOnly();
            this.ReturnType = returnType;
            this.ReturnTypeConstructor = returnTypeConstructor.ToList().AsReadOnly();
            this.ReferenceBody = referenceBody.ToList().AsReadOnly();
            this.DereferenceBody = dereferenceBody.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.ReferenceToken);
            this.ApplyArguments(compiler, arguments);

            foreach (Statement statement in this.ReturnTypeConstructor)
                statement.Compile(compiler);

            this.ReturnValue.Value = this.Subject.Variable.Value.Derive(
                this.ReturnType.Value, new FunctionalAddressOffset(this, arguments));

            compiler.TracePop();
        }

        public void CompileReference(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.ReferenceToken);
            this.ApplyArguments(compiler, arguments);

            foreach (Statement statement in this.ReferenceBody)
                statement.Compile(compiler);

            compiler.TracePop();
        }

        public void CompileDereference(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            compiler.TracePush(this.ReferenceToken);
            this.ApplyArguments(compiler, arguments);

            foreach (Statement statement in this.DereferenceBody)
                statement.Compile(compiler);

            compiler.TracePop();
        }
    }
}
