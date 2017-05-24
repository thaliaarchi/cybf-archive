using System.Collections.Generic;
using CyBF.BFC.Compilation;
using System.Linq;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;
using System.Text;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class CharacterDefinition : TypeDefinition
    {
        public const string StaticName = "Character";

        public CharacterDefinition() 
            : base(
                  new TypeConstraint(StaticName),
                  new SystemVariable())
        {
        }

        protected override TypeInstance OnCompile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            int arg = ((ConstInstance)valueArguments.Single().DataType).Value;

            if (arg < 0 || 255 < arg)
                compiler.RaiseSemanticError("Character argument out of range [0-255].");

            byte ordinal = (byte)arg;
            char character = Encoding.ASCII.GetChars(new byte[] { ordinal })[0];

            return new CharacterInstance(@"'\x" + arg.ToString("X2") + "'", character, ordinal);
        }
    }
}
