using CyBF.Parsing;

namespace CyBF.BFC.Model.Data
{
    public class UserVariable : Variable
    {
        public Token Reference { get; private set; }

        public UserVariable(Token reference, string name)
        {
            this.Reference = reference;
            this.Name = name;
        }
    }
}
