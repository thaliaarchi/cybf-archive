﻿using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Statements.Expressions;

namespace CyBF.BFC.Model.Statements
{
    public class WhileLoopStatement : Statement
    {
        public ExpressionStatement ConditionExpression { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public WhileLoopStatement(Token reference, ExpressionStatement conditionExpression, IEnumerable<Statement> body) 
            : base(reference)
        {
            this.ConditionExpression = conditionExpression;
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ConditionExpression.Compile(compiler);
            BFObject conditionObject = this.ConditionExpression.ReturnVariable.Value;

            if (!(conditionObject.DataType is ByteInstance))
            {
                compiler.TracePush(this.Reference);
                compiler.RaiseSemanticError(string.Format(
                    "Condition expression evaluates to '{0}'. Must evaluate to Byte.",
                    conditionObject.DataType.ToString()));
            }

            bool isVolatile = this.ConditionExpression.IsVolatile();

            BFObject controlObject = isVolatile ?
                compiler.CopyByte(conditionObject) : conditionObject;

            compiler.MoveToObject(controlObject);
            compiler.BeginCheckedLoop();

            foreach (Statement statement in this.Body)
                statement.Compile(compiler);

            this.ConditionExpression.Compile(compiler);
            conditionObject = this.ConditionExpression.ReturnVariable.Value;
            
            if (isVolatile)
                compiler.CopyByte(conditionObject, controlObject);
            else
                controlObject = conditionObject;

            compiler.MoveToObject(controlObject);
            compiler.EndCheckedLoop();
        }
    }
}
