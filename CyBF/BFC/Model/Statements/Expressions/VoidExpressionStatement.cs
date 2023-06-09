﻿using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using System;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class VoidExpressionStatement : ExpressionStatement
    {
        public VoidExpressionStatement(Token reference) 
            : base(reference)
        {
        }

        public override void Compile(BFCompiler compiler)
        {
            this.ReturnVariable.Value = new BFObject(new VoidInstance());
        }

        public override bool IsVolatile()
        {
            return false;
        }
    }
}
