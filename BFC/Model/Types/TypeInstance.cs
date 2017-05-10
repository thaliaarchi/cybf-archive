using System.Collections.Generic;
using System.Linq;
using CyBF.Utility;
using System;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Compilation;
using CyBF.BFC.Model.Data;

namespace CyBF.BFC.Model.Types
{
    public abstract class TypeInstance
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<TypeInstance> TypeArguments { get; private set; }
        public IReadOnlyList<FieldInstance> Fields { get; private set; }
        public DefinitionLibrary<FunctionDefinition> MethodLibrary { get; private set; }

        public TypeInstance(string typeName)
        {
            this.TypeName = typeName;
            this.TypeArguments = new List<TypeInstance>(0).AsReadOnly();
            this.Fields = new List<FieldInstance>(0).AsReadOnly();
            this.MethodLibrary = new DefinitionLibrary<FunctionDefinition>();
        }

        public TypeInstance(string typeName, IEnumerable<TypeInstance> typeArguments, IEnumerable<FieldInstance> fields)
            : this(typeName, typeArguments, fields, new DefinitionLibrary<FunctionDefinition>())
        {
        }

        public TypeInstance(
            string typeName, 
            IEnumerable<TypeInstance> typeArguments, 
            IEnumerable<FieldInstance> fields, 
            DefinitionLibrary<FunctionDefinition> methods)
        {
            this.TypeName = typeName;
            this.TypeArguments = typeArguments.ToList().AsReadOnly();

            List<FieldInstance> fieldList = new List<FieldInstance>();

            foreach (FieldInstance field in fields)
            {
                if (fieldList.Any(f => f.Name == field.Name))
                    throw new ArgumentException("Multiple fields with the same name specified.");

                fieldList.Add(field);
            }

            this.Fields = fieldList.AsReadOnly();

            this.MethodLibrary = methods;
        }

        public bool Matches(TypeInstance target)
        {
            return this.TypeName == target.TypeName &&
                   this.TypeArguments.MatchSequence(target.TypeArguments, (x, y) => x.Matches(y));
        }

        public bool ContainsField(string fieldName)
        {
            return this.Fields.Any(f => f.Name == fieldName);
        }

        public FieldInstance GetField(string fieldName)
        {
            foreach (FieldInstance field in this.Fields)
                if (field.Name == fieldName)
                    return field;

            throw new KeyNotFoundException();
        }

        public override string ToString()
        {
            if (this.TypeArguments.Count > 0)
            {
                string arguments = string.Join(" ", this.TypeArguments.Select(a => a.ToString()));
                return "[" + this.TypeName + " " + arguments + "]";
            }
            else
            {
                return "[" + this.TypeName + "]";
            }
        }

        public abstract int Size();
    }
}
