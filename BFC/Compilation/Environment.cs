using CyBF.BFC.Model;
using CyBF.BFC.Model.Statements;
using CyBF.Utility;
using System;
using System.Collections.Generic;

namespace CyBF.BFC.Compilation
{
    public class Environment
    {
        private StackedDictionary<string, Variable> _symbolTable;
        private Stack<List<Statement>> _statementStack;
        private List<Statement> _currentStatements;

        public IReadOnlyList<Statement> CurrentStatements
        {
            get
            {
                return _currentStatements.AsReadOnly();
            }
        }

        public Environment()
        {
            _symbolTable = new StackedDictionary<string, Variable>();
            _statementStack = new Stack<List<Statement>>();
            _currentStatements = new List<Statement>();
        }

        public void Define(Variable variable)
        {
            _symbolTable.Add(variable.Name, variable);
        }

        public bool Defines(string variableName)
        {
            return _symbolTable.ContainsKey(variableName);
        }

        public Variable Lookup(string variableName)
        {
            return _symbolTable[variableName];
        }

        public bool TryLookup(string variableName, out Variable variable)
        {
            return _symbolTable.TryGetValue(variableName, out variable);
        }

        public void Append(Statement statement)
        {
            _currentStatements.Add(statement);
        }

        public void Push()
        {
            _symbolTable.Push();
            _statementStack.Push(_currentStatements);
            _currentStatements = new List<Statement>();
        }

        public List<Statement> Pop()
        {
            if (_statementStack.Count == 0)
                throw new InvalidOperationException("Attempted to pop top-level environment frame.");

            _symbolTable.Pop();

            List<Statement> poppedStatements = _currentStatements;
            _currentStatements = _statementStack.Pop();

            return poppedStatements;
        }
    }
}
