using System.Collections.Generic;

namespace CyBF.BFIL
{
    public class BFILAssembler
    {
        public string AssembleProgram(BFILProgram program)
        {
            BFStringBuilder bfoutput = new BFStringBuilder();
            ReferenceTable variables = ProcessProgramVariables(program);
            int currentVariableAddress = 0;

            foreach (BFILStatement statement in program.Statements)
                statement.Compile(bfoutput, variables, ref currentVariableAddress);

            return bfoutput.ToString();
        }
        
        public ReferenceTable ProcessProgramVariables(BFILProgram program)
        {
            ReferenceTable orderedReferences = new ReferenceTable();
            MemoryAllocator allocator = new MemoryAllocator();

            List<Variable> reservedVariables = BuildOrderedReferences(program.Statements, orderedReferences);

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

            return orderedReferences;
        }
        
        public List<Variable> BuildOrderedReferences(IEnumerable<BFILStatement> statements, ReferenceTable orderedReferences)
        {
            Dictionary<string, Variable> localVariables = new Dictionary<string, Variable>();
            List<Variable> outerVariables = new List<Variable>();

            foreach (BFILStatement statement in statements)
            {
                if (statement is BFILDeclarationStatement)
                {
                    BFILDeclarationStatement declaration = (BFILDeclarationStatement)statement;

                    if (localVariables.ContainsKey(declaration.Name))
                        throw new BFILProgramError(declaration.ReferenceToken, "Duplicate variable definition.");

                    Variable variable = new Variable(declaration.Name, declaration.Size);
                    localVariables[variable.Name] = variable;
                    orderedReferences.Add(variable);
                }
                else if (statement is BFILReferenceStatement)
                {
                    BFILReferenceStatement reference = (BFILReferenceStatement)statement;

                    if (!orderedReferences.Contains(reference.Name))
                        throw new BFILProgramError(reference.ReferenceToken, "Undeclared variable.");

                    if (localVariables.ContainsKey(reference.Name))
                        orderedReferences.Add(localVariables[reference.Name]);
                }
                else if (statement is BFILLoopStatement)
                {
                    BFILLoopStatement loop = (BFILLoopStatement)statement;
                    List<Variable> loopOuterVariables = BuildOrderedReferences(loop.Body, orderedReferences);
                    
                    foreach (Variable variable in loopOuterVariables)
                    {
                        if (localVariables.ContainsKey(variable.Name))
                            orderedReferences.Add(variable);
                        else
                            outerVariables.Add(variable);
                    }
                }
            }

            return outerVariables;
        }
    }
}
