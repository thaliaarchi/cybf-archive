using System.Collections.Generic;

namespace CyBF.BFI
{
    public abstract class Node
    {
        public abstract void Compile(List<int> programInstructions);
    }
}
