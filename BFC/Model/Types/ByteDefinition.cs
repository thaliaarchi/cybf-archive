﻿using System.Collections.Generic;
using CyBF.BFC.Compilation;

namespace CyBF.BFC.Model.Types
{
    public class ByteDefinition : TypeDefinition
    {
        public const string StaticName = "Byte";

        public ByteDefinition()
            : base(
                new TypeConstraint(StaticName, new TypeParameter[] { }), 
                new Variable[] { })
        {
        }

        public override TypeInstance Compile(BFCompiler compiler, IEnumerable<TypeInstance> typeArguments, IEnumerable<BFObject> valueArguments)
        {
            this.ApplyArguments(compiler, typeArguments, valueArguments);
            return new ByteInstance();
        }
    }
}
