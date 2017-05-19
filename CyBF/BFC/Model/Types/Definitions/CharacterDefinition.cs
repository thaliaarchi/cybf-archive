using System.Collections.Generic;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Types.Instances;

namespace CyBF.BFC.Model.Types.Definitions
{
    public class CharacterDefinition : TypeDefinition
    {
        public const string StaticName = "Character";

        public CharacterDefinition() 
            : base(new TypeConstraint(StaticName))
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            return new CharacterInstance(@"'\x00'", '\0', 0);
        }
    }
}
