using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;

namespace CyBF.BFC.Model.Statements
{
    public class IterateStatement : Statement
    {
        public Variable ControlVariable { get; private set; }
        public ExpressionStatement LimitExpression { get; private set; }
        public IReadOnlyList<Statement> Body { get; private set; }

        public IterateStatement(
            Token reference, 
            Variable controlVariable, 
            ExpressionStatement limitExpression, 
            IEnumerable<Statement> body) 
            : base(reference)
        {
            this.ControlVariable = controlVariable;
            this.LimitExpression = limitExpression;
            this.Body = body.ToList().AsReadOnly();
        }

        public override void Compile(BFCompiler compiler)
        {
            this.LimitExpression.Compile(compiler);

            ConstInstance limitDataType = 
                this.LimitExpression.ReturnVariable.Value.DataType as ConstInstance;

            if (limitDataType == null)
                throw new SemanticError("Iteration limit does not evaluate to a Const.", this.Reference);

            int limit = limitDataType.Value;
            
            for (int i = 0; i < limit; i++)
            {
                this.ControlVariable.Value = new BFObject(new ConstInstance(i));

                foreach (Statement statement in this.Body)
                    statement.Compile(compiler);
            }
        }
    }
}
