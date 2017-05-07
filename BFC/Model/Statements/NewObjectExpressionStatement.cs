using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.Parsing;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Statements
{
    public class NewObjectExpressionStatement : ExpressionStatement
    {
        public TypeExpressionStatement TypeExpression { get; private set; }

        public NewObjectExpressionStatement(Token reference, TypeExpressionStatement typeExpression) 
            : base(reference)
        {
            this.TypeExpression = typeExpression;
        }

        public override void Compile(BFCompiler compiler)
        {
            this.TypeExpression.Compile(compiler);
            TypeInstance dataType = this.TypeExpression.ReturnVariable.Value;
            BFObject bfobject = compiler.MakeAndMoveToObject(dataType);

            List<FunctionDefinition> initializeFunctions = compiler.MatchFunction("initialize", new TypeInstance[] { dataType });
            
            if (initializeFunctions.Count > 1)
            {
                List<Token> references = new List<Token>();
                references.Add(this.Reference);

                foreach (FunctionDefinition definition in initializeFunctions)
                {
                    if (definition is ProcedureDefinition)
                        references.Add(((ProcedureDefinition)definition).Reference);
                }

                throw new SemanticError("Unable to resolve unique initialize function for type.", references);
            }
            else if (initializeFunctions.Count == 1)
            {
                FunctionDefinition definition = initializeFunctions[0];
                definition.Compile(compiler, new BFObject[] { bfobject });
            }

            this.ReturnVariable.Value = bfobject;
        }
    }
}
