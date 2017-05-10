using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Functions;

namespace CyBF.BFC.Model.Statements
{
    public class MethodCallExpressionStatement : ExpressionStatement
    {
        public string MethodName { get; private set; }
        public ExpressionStatement Source { get; private set; }
        public IReadOnlyList<ExpressionStatement> Arguments { get; private set; }

        public MethodCallExpressionStatement(
            Token reference,
            string methodName,
            ExpressionStatement source,
            IEnumerable<ExpressionStatement> arguments)
            : base(reference)
        {
            this.MethodName = methodName;
            this.Source = source;
            this.Arguments = arguments.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            this.Source.Compile(compiler);

            foreach (ExpressionStatement argument in this.Arguments)
                argument.Compile(compiler);

            BFObject sourceObject = this.Source.ReturnVariable.Value;
            IEnumerable<BFObject> methodArgumentObjects = this.Arguments.Select(arg => arg.ReturnVariable.Value);

            compiler.TracePush(this.Reference);

            FunctionDefinition method = compiler.ResolveMethod(this.MethodName, sourceObject, methodArgumentObjects);

            IEnumerable<BFObject> combinedArguments =
                new BFObject[] { sourceObject }.Concat(methodArgumentObjects);

            this.ReturnVariable.Value = method.Compile(compiler, combinedArguments);

            compiler.TracePop();
        }

        public override bool IsVolatile()
        {
            return true;
        }
    }
}
