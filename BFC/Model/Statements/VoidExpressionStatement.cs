﻿using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
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
    }
}
