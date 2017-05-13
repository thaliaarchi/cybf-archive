using System;
using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types.Instances;
using CyBF.BFC.Model.Data;
using System.Text;

namespace CyBF.BFC.Model.Statements.Expressions
{
    public class ToStringExpressionStatement : ExpressionStatement
    {
        public ExpressionStatement Expression { get; private set; }
        public TypeExpressionStatement TypeExpression { get; private set; }

        public ToStringExpressionStatement(Token reference, ExpressionStatement expression) 
            : base(reference)
        {
            this.Expression = expression;
            this.TypeExpression = null;
        }

        public ToStringExpressionStatement(Token reference, TypeExpressionStatement typeExpression)
            : base(reference)
        {
            this.Expression = null;
            this.TypeExpression = typeExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            TypeInstance datatype;

            if (this.Expression != null)
            {
                this.Expression.Compile(compiler);
                datatype = this.Expression.ReturnVariable.Value.DataType;
            }
            else if (this.TypeExpression != null)
            {
                this.TypeExpression.Compile(compiler);
                datatype = this.TypeExpression.ReturnVariable.Value;
            }
            else
            {
                throw new InvalidOperationException();
            }

            string stringRepresentation = BuildStringRepresentation(datatype);
            string rawRepresentation = BuildRawRepresentation(stringRepresentation);

            this.ReturnVariable.Value = new BFObject(new StringInstance(rawRepresentation, stringRepresentation));
        }

        private string BuildStringRepresentation(TypeInstance datatype)
        {
            if (datatype is CharacterInstance)
            {
                return ((CharacterInstance)datatype).RawString;
            }
            else if (datatype is ConstInstance)
            {
                return ((ConstInstance)datatype).Value.ToString();
            }
            else if (datatype is StringInstance)
            {
                return ((StringInstance)datatype).RawString;
            }
            else if (datatype is TupleInstance)
            {
                TupleInstance tupleInstance = (TupleInstance)datatype;
                IEnumerable<TypeInstance> elementTypes = tupleInstance.Elements.Select(obj => obj.DataType);
                IEnumerable<string> elementTypeRepresentations = elementTypes.Select(t => BuildStringRepresentation(t));

                return "(, " + string.Join(", ", elementTypeRepresentations) + ")";
            }
            else
            {
                return datatype.ToString();
            }
        }
        
        private string BuildRawRepresentation(string stringRepresentation)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(stringRepresentation);
            string raw = "\"" + string.Join("", bytes.Select(b => @"\x" + ((int)b).ToString("X2"))) + "\"";

            return raw;
        }

        public override bool IsVolatile()
        {
            // Should be non-volatile for the same reason that "sizeof" expressions are.
            // Namely, the data type shouldn't change even if the contents of the BFObject do.

            return false;
        }
    }
}
