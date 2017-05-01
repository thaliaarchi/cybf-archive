﻿using CyBF.BFC.Model;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Statements.Commands;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyBF.BFC.Compilation
{
    public class ModelBuilder
    {
        private Parser _parser;
        private DefinitionLibrary _library;
        private Environment _environment;
        
        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
            _library = new DefinitionLibrary();
            _environment = new Environment();

            _library.DefineFunction(new ConstAddOperatorDefinition());

            _library.DefineType(new ByteDefinition());
            _library.DefineType(new ConstDefinition());
            _library.DefineType(new ArrayDefinition());
        }

        public string Compile()
        {
            BFCompiler compiler = new BFCompiler(_library);

            foreach (Statement statement in _environment.CurrentStatements)
                statement.Compile(compiler);

            return compiler.GetCode();
        }

        public Variable LookupVariable(Token variableNameToken)
        {
            string variableName = variableNameToken.ProcessedValue;
            Variable variable;

            if (!_environment.TryLookup(variableName, out variable))
                throw new SemanticError("Variable not defined.", variableNameToken);

            return variable;
        }

        public TypeVariable LookupTypeVariable(Token typeVariableNameToken)
        {
            string typeVariableName = typeVariableNameToken.ProcessedValue;
            TypeVariable typeVariable;

            if (!_environment.TryLookup(typeVariableName, out typeVariable))
                throw new SemanticError("Type variable not defined.", typeVariableNameToken);

            return typeVariable;
        }

        public void ParseProgram()
        {
            while (!_parser.Matches(TokenType.EndOfSource))
            {
                if (_parser.Matches(TokenType.Keyword_Def))
                    ParseProcedureDefinition();
                else
                    ParseStatement();
            }
        }

        public void ParseProcedureDefinition()
        {
            _environment.Push();

            Token reference = _parser.Match(TokenType.Keyword_Def);
            string functionName = _parser.Match(TokenType.Identifier, TokenType.Operator).ProcessedValue;

            _parser.Match(TokenType.OpenParen);

            List<FunctionParameter> parameters = new List<FunctionParameter>();

            if (!_parser.Matches(TokenType.CloseParen))
            {
                parameters.Add(ParseFunctionParameter());

                while (_parser.Matches(TokenType.Comma))
                {
                    _parser.Next();
                    parameters.Add(ParseFunctionParameter());
                }
            }

            _parser.Match(TokenType.CloseParen);

            while (!_parser.Matches(TokenType.Keyword_Return, TokenType.Keyword_End))
                ParseStatement();

            Variable returnValue;

            if (_parser.Matches(TokenType.Keyword_Return))
            {
                _parser.Next();
                returnValue = ParseExpression();
                _parser.Match(TokenType.Semicolon);
            }
            else
            {
                returnValue = new Variable();
                _environment.Append(new ConstStatement(reference, 0, returnValue));
            }

            _parser.Match(TokenType.Keyword_End);

            IEnumerable<Statement> body = _environment.CurrentStatements;

            ProcedureDefinition definition = new ProcedureDefinition(
                reference, functionName, returnValue, parameters, body);

            _library.DefineFunction(definition);

            _environment.Pop();
        }

        public FunctionParameter ParseFunctionParameter()
        {
            string variableName = _parser.Match(TokenType.Identifier).ProcessedValue;
            Variable variable = new Variable(variableName);
            _environment.Define(variable);

            _parser.Match(TokenType.Colon);

            TypeParameter typeParameter = ParseTypeParameter();

            return new FunctionParameter(variable, typeParameter);
        }

        public TypeParameter ParseTypeParameter()
        {
            if (_parser.MatchesLookahead(TokenType.TypeVariable, TokenType.Colon))
            {
                TypeVariable typeVariable = ParseTypeParameterVariable();
                _parser.Match(TokenType.Colon);
                TypeConstraint constraint = ParseTypeConstraint();

                return new ConstrainedTypeParameter(typeVariable, constraint);
            }
            else if (_parser.Matches(TokenType.TypeVariable))
            {
                TypeVariable typeVariable = ParseTypeParameterVariable();
                return new TypeParameter(typeVariable);
            }
            else
            {
                TypeConstraint constraint = ParseTypeConstraint();
                return new ConstrainedTypeParameter(constraint);
            }
        }
        
        public TypeVariable ParseTypeParameterVariable()
        {
            string typeVariableName = _parser.Match(TokenType.TypeVariable).ProcessedValue;
            TypeVariable typeVariable;

            if (!_environment.TryLookup(typeVariableName, out typeVariable))
            {
                typeVariable = new TypeVariable(typeVariableName);
                _environment.Define(typeVariable);
            }

            return typeVariable;
        }

        public TypeConstraint ParseTypeConstraint()
        {
            if (_parser.Matches(TokenType.Identifier))
            {
                string typeName = _parser.Next().ProcessedValue;
                return new TypeConstraint(typeName);
            }
            else
            {
                _parser.Match(TokenType.OpenBracket);

                string typeName = _parser.Next().ProcessedValue;

                List<TypeParameter> parameters = new List<TypeParameter>();

                while (!_parser.Matches(TokenType.CloseBracket))
                    parameters.Add(ParseTypeParameter());

                _parser.Match(TokenType.CloseBracket);

                return new TypeConstraint(typeName, parameters);
            }
        }

        public void ParseStatement()
        {
            switch (_parser.Current.TokenType)
            {
                case TokenType.Keyword_Let:
                    ParseVariableAssignmentStatement();
                    break;

                case TokenType.Keyword_Var:
                    ParseVariableDeclarationStatement();
                    break;

                case TokenType.OpenBrace:
                    ParseCommandBlockStatement();
                    break;

                default:
                    ParseExpression();
                    _parser.Match(TokenType.Semicolon);
                    break;
            }
        }

        public void ParseVariableDeclarationStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Var);
            string varName = _parser.Match(TokenType.Identifier).ProcessedValue;
            _parser.Match(TokenType.Colon);
            TypeVariable dataType = ParseTypeExpression();

            Variable variable = new Variable(varName);
            _environment.Define(variable);

            _environment.Append(new VariableDeclarationStatement(reference, variable, dataType));
        }

        public TypeVariable ParseTypeExpression()
        {
            if (_parser.Matches(TokenType.TypeVariable))
                return LookupTypeVariable(_parser.Next());
            
            return ParseTypeConstructor();
        }

        public TypeVariable ParseTypeConstructor()
        {
            Token reference;
            string typeName;
            List<TypeVariable> typeArguments = new List<TypeVariable>();
            List<Variable> valueArguments = new List<Variable>();
            TypeVariable returnValue = new TypeVariable();

            if (_parser.Matches(TokenType.Identifier))
            {
                reference = _parser.Next();
                typeName = reference.ProcessedValue;
            }
            else
            {
                reference = _parser.Match(TokenType.OpenBracket);
                typeName = _parser.Match(TokenType.Identifier).ProcessedValue;

                while (!_parser.Matches(TokenType.CloseBracket))
                    typeArguments.Add(ParseTypeExpression());

                _parser.Match(TokenType.CloseBracket);
            }

            if (_parser.Matches(TokenType.OpenParen))
            {
                _parser.Next();

                if (!_parser.Matches(TokenType.CloseParen))
                {
                    valueArguments.Add(ParseExpression());

                    while (_parser.Matches(TokenType.Comma))
                    {
                        _parser.Next();
                        valueArguments.Add(ParseExpression());
                    }
                }

                _parser.Match(TokenType.CloseParen);
            }

            _environment.Append(new TypeConstructionStatement(
                reference,
                typeName,
                typeArguments,
                valueArguments,
                returnValue));

            return returnValue;
        }

        public void ParseVariableAssignmentStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Let);

            Token identifier = _parser.Match(TokenType.Identifier);
            Variable declaredVariable = new Variable(identifier.ProcessedValue);
            _environment.Define(declaredVariable);

            _parser.Match(TokenType.Colon);
            Variable expressionResult = ParseExpression();
            _parser.Match(TokenType.Semicolon);

            Statement statement = new VariableAssignmentStatement(reference, declaredVariable, expressionResult);
            _environment.Append(statement);
        }

        public void ParseCommandBlockStatement()
        {
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenBrace);

            while (!_parser.Matches(TokenType.CloseBrace))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseBrace);

            _environment.Append(new CommandBlockStatement(reference, commands));
        }

        public Command ParseCommand()
        {
            if (_parser.Matches(TokenType.OpenBracket))
                return ParseLoopCommand();

            if (_parser.Matches(TokenType.OpenParen))
                return ParseRepeatCommand();

            if (_parser.Matches(TokenType.Identifier))
                return ParseVariableReferenceCommand();

            if (_parser.Matches(TokenType.Hash))
                return ParseWriteCommand();

            return ParseOperatorStringCommand();
        }

        public LoopCommand ParseLoopCommand()
        {
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenBracket);

            while (!_parser.Matches(TokenType.CloseBracket))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseBracket);

            return new LoopCommand(reference, commands);
        }

        public OperatorStringCommand ParseOperatorStringCommand()
        {
            StringBuilder opstring = new StringBuilder();

            TokenType[] operatorStringTokens = new TokenType[]
            {
                TokenType.Plus,
                TokenType.Minus,
                TokenType.OpenAngle,
                TokenType.CloseAngle,
                TokenType.Comma,
                TokenType.Period
            };

            Token reference = _parser.Match(operatorStringTokens);
            opstring.Append(reference.ProcessedValue);

            while (_parser.Matches(operatorStringTokens))
                opstring.Append(_parser.Next().ProcessedValue);

            return new OperatorStringCommand(reference, opstring.ToString());
        }

        public RepeatCommand ParseRepeatCommand()
        {
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenParen);

            while (!_parser.Matches(TokenType.CloseParen))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseParen);
            _parser.Match(TokenType.Asterisk);

            Variable counterVariable = _parser.Matches(TokenType.Identifier) ?
                LookupVariable(_parser.Next()) : ParseLiteralExpression();
            
            return new RepeatCommand(reference, commands, counterVariable);
        }

        public VariableReferenceCommand ParseVariableReferenceCommand()
        {
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = LookupVariable(variableNameToken);

            return new VariableReferenceCommand(variableNameToken, variable);
        }

        public WriteCommand ParseWriteCommand()
        {
            Token reference = _parser.Match(TokenType.Hash);
            List<Variable> variables = new List<Variable>();

            bool writeList = _parser.Matches(TokenType.OpenParen);

            if (writeList)
                _parser.Next();

            Variable variable = _parser.Matches(TokenType.Identifier) ?
                LookupVariable(_parser.Next()) : ParseLiteralExpression();

            variables.Add(variable);

            if (writeList)
            {
                while (_parser.Matches(TokenType.Comma))
                {
                    _parser.Next();

                    variable = _parser.Matches(TokenType.Identifier) ?
                        LookupVariable(_parser.Next()) : ParseLiteralExpression();

                    variables.Add(variable);
                }

                _parser.Match(TokenType.CloseParen);
            }

            return new WriteCommand(reference, variables);
        }
        
        public Variable ParseExpression()
        {
            Variable left = ParseUnaryExpression();

            if (_parser.Matches(TokenType.Operator))
            {
                Token operatorToken = _parser.Next();
                Variable right = ParseExpression();
                Variable returnValue = new Variable();

                _environment.Append(
                    new FunctionCallStatement(
                        operatorToken,
                        operatorToken.ProcessedValue,
                        new Variable[] { left, right },
                        returnValue));

                return returnValue;
            }

            return left;
        }

        public Variable ParseUnaryExpression()
        {
            if (_parser.Matches(TokenType.Operator))
            {
                Token operatorToken = _parser.Next();
                Variable argument = ParseUnaryExpression();
                Variable returnValue = new Variable();

                _environment.Append(
                    new FunctionCallStatement(
                        operatorToken, 
                        operatorToken.ProcessedValue, 
                        new Variable[] { argument }, 
                        returnValue));

                return returnValue;
            }

            return ParseAtomicExpression();
        }

        public Variable ParseAtomicExpression()
        {
            Variable returnValue;

            if (_parser.Matches(TokenType.Numeric, TokenType.String))
                returnValue = ParseLiteralExpression();

            else if (_parser.Matches(TokenType.OpenParen))
                returnValue = ParseParenthesizedExpression();

            else if (_parser.MatchesLookahead(TokenType.Identifier, TokenType.OpenParen))
                returnValue = ParseFunctionCallExpression();
            
            else
                returnValue = ParseVariableExpression();
            
            while (_parser.Matches(TokenType.Period))
            {
                _parser.Next();
                Token fieldNameToken = _parser.Match(TokenType.Identifier);
                string fieldName = fieldNameToken.ProcessedValue;

                Variable source = returnValue;
                returnValue = new Variable();

                _environment.Append(new FieldReferenceStatement(fieldNameToken, source, fieldName, returnValue));
            }

            return returnValue;
        }

        public Variable ParseFunctionCallExpression()
        {
            Token nameToken = _parser.Match(TokenType.Identifier);
            List<Variable> arguments = new List<Variable>();
            Variable returnValue = new Variable();

            _parser.Match(TokenType.OpenParen);

            if (!_parser.Matches(TokenType.CloseParen))
            {
                arguments.Add(ParseExpression());

                while(_parser.Matches(TokenType.Comma))
                {
                    _parser.Next();
                    arguments.Add(ParseExpression());
                }
            }

            _parser.Match(TokenType.CloseParen);

            _environment.Append(new FunctionCallStatement(nameToken, nameToken.ProcessedValue, arguments, returnValue));

            return returnValue;
        }

        public Variable ParseVariableExpression()
        {
            Token nameToken = _parser.Match(TokenType.Identifier);
            return LookupVariable(nameToken);
        }

        public Variable ParseParenthesizedExpression()
        {
            _parser.Match(TokenType.OpenParen);
            Variable result = ParseExpression();
            _parser.Match(TokenType.CloseParen);

            return result;
        }

        public Variable ParseLiteralExpression()
        {
            Token token = _parser.Match(TokenType.String, TokenType.Numeric);
            Variable returnValue = new Variable();
            Statement statement;

            if (token.TokenType == TokenType.String)
                statement = new StringStatement(token, new CyBFString(token.RawValue, token.ProcessedValue), returnValue);
            else
                statement = new ConstStatement(token, token.NumericValue, returnValue);

            _environment.Append(statement);

            return returnValue;
        }
    }
}
