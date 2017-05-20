using System.Collections.Generic;
using System.Text;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types;
using CyBF.BFC.Model.Types.Definitions;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Functions.Builtins
{
    public class StringIndexOperatorDefinition : FunctionDefinition
    {
        public StringIndexOperatorDefinition()
            : base("[]", 
                new TypeConstraint(StringDefinition.StaticName),
                new TypeConstraint(ConstDefinition.StaticName))
        {
        }

        protected override BFObject OnCompile(BFCompiler compiler, IEnumerable<BFObject> arguments)
        {
            this.ApplyArguments(compiler, arguments);

            List<BFObject> arglist = new List<BFObject>(arguments);
            string processedString = ((StringInstance)arglist[0].DataType).ProcessedString;
            int index = ((ConstInstance)arglist[1].DataType).Value;

            if (index < 0 || processedString.Length <= index)
            {
                compiler.RaiseSemanticError(string.Format(
                    "Index {0} out of range for string literal of size {1}.",
                    index, processedString.Length));
            }

            char character = processedString[index];
            byte ordinal = Encoding.ASCII.GetBytes(new char[] { processedString[index] })[0];
            string literalString = @"'\x" + ordinal.ToString("X2") + "'";

            return new BFObject(new CharacterInstance(literalString, character, ordinal));
        }
    }
}
