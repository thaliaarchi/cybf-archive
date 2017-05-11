using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Statements.Expressions;
using CyBF.BFC.Model.Types.Instances;
using CyBF.Parsing;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class FieldDefinition
    {
        public Token Reference { get; private set; }
        public string Name { get; private set; }
        public TypeExpressionStatement DataTypeExpression { get; private set; }

        public FieldDefinition(Token reference, string name, TypeExpressionStatement dataTypeExpression)
        {
            this.Reference = reference;
            this.Name = name;
            this.DataTypeExpression = dataTypeExpression;
        }

        public FieldInstance Compile(BFCompiler compiler, int offset)
        {
            this.DataTypeExpression.Compile(compiler);
            TypeInstance dataType = this.DataTypeExpression.ReturnVariable.Value;

            return new FieldInstance(this.Name, dataType, offset);
        }
    }
}
