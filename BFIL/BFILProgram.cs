using System.Collections.Generic;
using System.Linq;

namespace CyBF.BFIL
{
    public class BFILProgram
    {
        public IReadOnlyList<BFILStatement> Statements { get; private set; }

        public BFILProgram(IEnumerable<BFILStatement> statements)
        {
            this.Statements = statements.ToList().AsReadOnly();
        }
    }
}
