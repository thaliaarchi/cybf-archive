using System.Collections.Generic;
using System.Linq;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Addressing;

namespace CyBF.BFC.Model.Statements
{
    public class FieldReferenceStatement : Statement
    {
        public Variable Source { get; private set; }
        public string FieldName { get; private set; }
        public Variable ReturnValue { get; private set; }

        public FieldReferenceStatement(Token reference, Variable source, string fieldName, Variable returnValue) 
            : base(reference)
        {
            this.Source = source;
            this.FieldName = fieldName;
            this.ReturnValue = returnValue;
        }

        public override void Compile(BFCompiler compiler)
        {
            FieldInstance field = ResolveField();
            NumericAddressOffset fieldOffset = new NumericAddressOffset(field.Offset);
            this.ReturnValue.Value = this.Source.Value.Derive(field.DataType, fieldOffset);
        }

        private FieldInstance ResolveField()
        {
            TypeInstance sourceDataType = this.Source.Value.DataType;
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
