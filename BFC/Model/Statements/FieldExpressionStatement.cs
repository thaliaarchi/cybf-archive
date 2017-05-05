using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Addressing;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class FieldExpressionStatement : ExpressionStatement
    {
        public ExpressionStatement Source { get; private set; }
        public string FieldName { get; private set; }

        public FieldExpressionStatement(Token reference, ExpressionStatement source, string fieldName) 
            : base(reference)
        {
            this.Source = source;
            this.FieldName = fieldName;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.Source.Compile(compiler);
            BFObject sourceObject = this.Source.ReturnVariable.Value;

            FieldInstance field = ResolveField(sourceObject);
            NumericAddressOffset fieldOffset = new NumericAddressOffset(field.Offset);
            this.ReturnVariable.Value = sourceObject.Derive(field.DataType, fieldOffset);
        }

        private FieldInstance ResolveField(BFObject sourceObject)
        {
            TypeInstance sourceDataType = sourceObject.DataType;
            List<Token> referenceTokens = new List<Token>();

            referenceTokens.Add(this.Reference);

            if (sourceDataType is StructInstance)
                referenceTokens.Add(((StructInstance)sourceDataType).Reference);

            if (!sourceDataType.ContainsField(this.FieldName))
            {
                throw new SemanticError(
                    string.Format("Field '{0}' not defined on type '{1}'.", this.FieldName, sourceDataType),
                    referenceTokens);
            }

            return sourceDataType.GetField(this.FieldName);
        }
    }
}
