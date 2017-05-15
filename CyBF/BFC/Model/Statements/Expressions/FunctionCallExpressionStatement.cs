using CyBF.Parsing;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Data;
using System;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class FunctionCallExpressionStatement : ExpressionStatement
    {
        public string FunctionName { get; private set; }
        public IReadOnlyList<ExpressionStatement> Arguments { get; private set; }

        public FunctionCallExpressionStatement(
            Token reference, 
            string functionName, 
            IEnumerable<ExpressionStatement> arguments)
            : base(reference)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            foreach (ExpressionStatement argument in this.Arguments)
                argument.Compile(compiler);

            IEnumerable<BFObject> argumentObjects = this.Arguments.Select(arg => arg.ReturnVariable.Value);

            compiler.TracePush(this.Reference);
            
            FunctionDefinition definition = compiler.ResolveFunction(this.FunctionName, argumentObjects);
            this.ReturnVariable.Value = definition.Compile(compiler, argumentObjects);

            compiler.TracePop();
        }

        public override bool IsVolatile()
        {
            return true;
        }
    }
}
