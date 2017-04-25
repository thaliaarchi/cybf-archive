using CyBF.BFC.Model;
using CyBF.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFC.Compilation
{
    public class Module
    {
        public Token Reference { get; private set; }
        public string Name { get; private set; }
        public IEnumerable<Token> Code { get; private set; }

        private List<Module> _dependencies = new List<Module>();
        public IReadOnlyList<Module> Dependencies { get { return _dependencies.AsReadOnly(); } }

        private bool _updatingRank;
        public int Rank { get; private set; }

        public Module(Token reference, string name, IEnumerable<Token> code)
        {
            this.Reference = reference;
            this.Name = name;
            this.Code = code;

            _updatingRank = false;
            this.Rank = 0;
        }

        public void AddDependency(Module parent)
        {
            _dependencies.Add(parent);
            parent.UpdateRank(this.Rank, new Stack<Module>());
        }

        private void UpdateRank(int childRank, Stack<Module> dependencyStack)
        {
            dependencyStack.Push(this);

            if (_updatingRank)
                throw new SemanticError("Circular module dependencies detected.", dependencyStack.Select(m => m.Reference));

            _updatingRank = true;

            if (this.Rank <= childRank)
            {
                this.Rank = childRank + 1;

                foreach (Module parent in _dependencies)
                    parent.UpdateRank(this.Rank, dependencyStack);
            }

            _updatingRank = false;

            dependencyStack.Pop();
        }
    }
}
