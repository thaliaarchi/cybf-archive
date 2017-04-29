using System.Collections.Generic;
using System.Text;

namespace CyBF.BFIL
{
    public class BFILAssembler
    {
        public string AssembleProgram(BFILProgram program, out string debuggingSource)
        {
            BFStringBuilder bfoutput = new BFStringBuilder();
            StringBuilder debugOutput = new StringBuilder();
            ReferenceTable variables = ProcessProgramVariables(program);

            int currentAddress = 0;

            foreach (BFILStatement statement in program.Statements)
            {
                statement.PrintDebugSource(debugOutput, variables, 0);
                statement.Compile(bfoutput, variables, ref currentAddress);
            }

            debuggingSource = debugOutput.ToString();
            return bfoutput.ToString();
        }

        public string AssembleProgram(BFILProgram program)
        {
            string debuggingSource;
            return AssembleProgram(program, out debuggingSource);
        }

        public ReferenceTable ProcessProgramVariables(BFILProgram program)
        {
            ReferenceTable orderedReferences = new ReferenceTable();
            MemoryAllocator allocator = new MemoryAllocator();

            Variable unallocatedMarker = new Variable("_UNALLOCATED_", 1);
            orderedReferences.RegisterWithoutReference(unallocatedMarker);

            IEnumerable<BFILReferenceStatement> undeclaredReferences = BuildOrderedReferences(program.Statements, orderedReferences);

            foreach (BFILReferenceStatement reference in undeclaredReferences)
                if (reference.Name != unallocatedMarker.Name)
                    throw new BFILProgramError(reference.ReferenceToken, "Variable undefined or referenced out of scope.");

            Dictionary<Variable, int> lastReferenceIndex = new Dictionary<Variable, int>();
            IReadOnlyList<Variable> referenceOrder = orderedReferences.GetReferenceOrder();

            for (int i = 0; i < referenceOrder.Count; i++)
                lastReferenceIndex[referenceOrder[i]] = i;

            for (int i = 0; i < referenceOrder.Count; i++)
            {
                Variable variable = referenceOrder[i];

                if (variable.Address < 0)
                {
                    variable.Address = allocator.Allocate(variable.Size);
                }
                else
                {
                    if (i == lastReferenceIndex[variable])
                        allocator.Free(variable.Address);
                }
            }

            unallocatedMarker.Address = allocator.AllocationMaximum;

            return orderedReferences;
        }
        
        public IEnumerable<BFILReferenceStatement> BuildOrderedReferences(IEnumerable<BFILStatement> statements, ReferenceTable orderedReferences)
        {
            Dictionary<string, Variable> localVariables = new Dictionary<string, Variable>();
            HashSet<BFILReferenceStatement> deferredReferences = new HashSet<BFILReferenceStatement>();

            foreach (BFILStatement statement in statements)
            {
                if (statement is BFILDeclarationStatement)
                {
                    BFILDeclarationStatement declaration = (BFILDeclarationStatement)statement;

                    if (orderedReferences.Contains(declaration.Name))
                        throw new BFILProgramError(declaration.ReferenceToken, "Duplicate variable definition.");

                    Variable variable = new Variable(declaration.Name, declaration.Size);
                    localVariables[variable.Name] = variable;
                    orderedReferences.Add(variable);
                }
                else if (statement is BFILReferenceStatement)
                {
                    BFILReferenceStatement reference = (BFILReferenceStatement)statement;

                    if (localVariables.ContainsKey(reference.Name))
                        orderedReferences.Add(localVariables[reference.Name]);
                    else
                        deferredReferences.Add(reference);
                }
                else if (statement is BFILLoopStatement)
                {
                    BFILLoopStatement loop = (BFILLoopStatement)statement;
                    IEnumerable<BFILReferenceStatement> loopDeferredReferences = BuildOrderedReferences(loop.Body, orderedReferences);

                    foreach (BFILReferenceStatement reference in loopDeferredReferences)
                    {
                        if (localVariables.ContainsKey(reference.Name))
                            orderedReferences.Add(localVariables[reference.Name]);
                        else
                            deferredReferences.Add(reference);
                    }
                }
            }

            return deferredReferences;
        }
    }
}
