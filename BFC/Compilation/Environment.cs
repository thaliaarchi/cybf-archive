using CyBF.BFC.Model;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Types;
using CyBF.Utility;
using System;
using System.Collections.Generic;

namespace CyBF.BFC.Compilation
{
    public class Environment
    {
        private StackedDictionary<string, Variable> _variableTable;
        private StackedDictionary<string, TypeVariable> _typeVariableTable;
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
            _variableTable = new StackedDictionary<string, Variable>();
            _typeVariableTable = new StackedDictionary<string, TypeVariable>();
            _statementStack = new Stack<List<Statement>>();
            _currentStatements = new List<Statement>();
        }

        public void Define(Variable variable)
        {
            _variableTable.Add(variable.Name, variable);
        }

        public void Define(TypeVariable typeVariable)
        {
            _typeVariableTable.Add(typeVariable.Name, typeVariable);
        }

        public bool DefinesVariable(string variableName)
        {
            return _variableTable.ContainsKey(variableName);
        }

        public bool DefinesTypeVariable(string typeVariableName)
        {
            return _typeVariableTable.ContainsKey(typeVariableName);
        }

        public Variable LookupVariable(string variableName)
        {
            return _variableTable[variableName];
        }

        public TypeVariable LookupTypeVariable(string typeVariableName)
        {
            return _typeVariableTable[typeVariableName];
        }

        public bool TryLookup(string variableName, out Variable variable)
        {
            return _variableTable.TryGetValue(variableName, out variable);
        }

        public bool TryLookup(string typeVariableName, out TypeVariable typeVariable)
        {
            return _typeVariableTable.TryGetValue(typeVariableName, out typeVariable);
        }

        public void Append(Statement statement)
        {
            _currentStatements.Add(statement);
        }

        public void Push()
        {
            _variableTable.Push();
            _typeVariableTable.Push();
            _statementStack.Push(_currentStatements);
            _currentStatements = new List<Statement>();
        }

        public List<Statement> Pop()
        {
            if (_statementStack.Count == 0)
                throw new InvalidOperationException("Attempted to pop top-level environment frame.");

            _variableTable.Pop();
            _typeVariableTable.Pop();

            List<Statement> poppedStatements = _currentStatements;
            _currentStatements = _statementStack.Pop();

            return poppedStatements;
        }
    }
}
