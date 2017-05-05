﻿using CyBF.BFC.Model;
using CyBF.BFC.Model.Data;
using CyBF.BFC.Model.Functions;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Statements.Commands;
using CyBF.BFC.Model.Types;
using CyBF.Parsing;
using CyBF.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyBF.BFC.Compilation
{
    public class ModelBuilder
    {
        private Parser _parser;
        private StackedDictionary<string, UserVariable> _symbolTable;
        private StackedDictionary<string, TypeVariable> _typeSymbolTable;
        
        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
            _symbolTable = new StackedDictionary<string, UserVariable>();
            _typeSymbolTable = new StackedDictionary<string, TypeVariable>();
        }

        public UserVariable LookupUserVariable(Token variableNameToken)
        {
            string variableName = variableNameToken.ProcessedValue;
            UserVariable variable;

            if (!_symbolTable.TryGetValue(variableName, out variable))
                throw new SemanticError("Variable not defined.", variableNameToken);

            return variable;
        }

        public TypeVariable LookupTypeVariable(Token typeVariableNameToken)
        {
            string typeVariableName = typeVariableNameToken.ProcessedValue;
            TypeVariable typeVariable;

            if (!_typeSymbolTable.TryGetValue(typeVariableName, out typeVariable))
                throw new SemanticError("Type variable not defined.", typeVariableNameToken);

            return typeVariable;
        }

        public Variable CreateEnvironmentVariable(Token variableNameToken)
        {
            string variableName = variableNameToken.ProcessedValue;
            
            if (_symbolTable.CurrentFrame.ContainsKey(variableName))
                throw new SemanticError("Duplicate variable definition.", variableNameToken);

            UserVariable variable = new UserVariable(variableNameToken, variableName);
            _symbolTable.Add(variableName, variable);

            return variable;
        }

        public CyBFProgram BuildProgram()
        {
            List<TypeDefinition> dataTypes = new List<TypeDefinition>();
            List<FunctionDefinition> functions = new List<FunctionDefinition>();
            List<Statement> statements = new List<Statement>();

            dataTypes.Add(new ByteDefinition());
            dataTypes.Add(new ConstDefinition());
            dataTypes.Add(new ArrayDefinition());
            dataTypes.Add(new VoidDefinition());
            dataTypes.Add(new StringDefinition());

            functions.Add(new ConstAddOperatorDefinition());

            while (!_parser.Matches(TokenType.EndOfSource))
            {
                switch (_parser.Current.TokenType)
                {
                    case TokenType.Keyword_Struct:
                        dataTypes.Add(ParseStructDefinition());
                        break;

                    case TokenType.Keyword_Function:
                        functions.Add(ParseProcedureDefinition());
                        break;

                    case TokenType.Keyword_Selector:
                        functions.Add(ParseSelectorDefinition());
                        break;

                    default:
                        statements.Add(ParseStatement());
                        break;
                }
            }

            return new CyBFProgram(dataTypes, functions, statements);
        }

        public StructDefinition ParseStructDefinition()
        {
            _symbolTable.Push();
            _typeSymbolTable.Push();

            Token reference = _parser.Match(TokenType.Keyword_Struct);
            TypeConstraint constraint = ParseTypeConstraint();
            List<Variable> parameters = new List<Variable>(0);

            if (_parser.Matches(TokenType.OpenParen))
            {
                List<Token> parameterNameTokens = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    () => _parser.Match(TokenType.Identifier));

                foreach (Token parameterNameToken in parameterNameTokens)
                {
                    Variable parameter = CreateEnvironmentVariable(parameterNameToken);
                    parameters.Add(parameter);
                }
            }

            List<FieldDefinition> fields = new List<FieldDefinition>();

            while (!_parser.Matches(TokenType.Keyword_End))
            {
                Token fieldNameToken = _parser.Match(TokenType.Identifier);
                string fieldName = fieldNameToken.ProcessedValue;

                _parser.Match(TokenType.Colon);

                TypeExpressionStatement fieldType = ParseTypeExpression();

                fields.Add(new FieldDefinition(fieldNameToken, fieldName, fieldType));
            }
            
            _parser.Match(TokenType.Keyword_End);

            StructDefinition definition = new StructDefinition(
                reference, constraint, parameters, fields);

            _typeSymbolTable.Pop();
            _symbolTable.Pop();

            return definition;
        }

        public SelectorDefinition ParseSelectorDefinition()
        {
            _symbolTable.Push();
            _typeSymbolTable.Push();

            Token referenceToken = _parser.Match(TokenType.Keyword_Selector);
            FunctionParameter sourceParameter = ParseFunctionParameter();

            List<FunctionParameter> indexParameters = _parser.ParseDelimitedList(
                TokenType.OpenBracket, TokenType.Comma, TokenType.CloseBracket, ParseFunctionParameter);

            _parser.Match(TokenType.Colon);

            TypeExpressionStatement returnTypeExpression = ParseTypeExpression();
            List<Statement> referenceBody = ParseSelectorBodySection(TokenType.Keyword_Reference);
            List<Statement> dereferenceBody = ParseSelectorBodySection(TokenType.Keyword_Dereference);

            _parser.Match(TokenType.Keyword_End);

            SelectorDefinition definition = new SelectorDefinition(
                referenceToken, sourceParameter, indexParameters, returnTypeExpression, referenceBody, dereferenceBody);

            _typeSymbolTable.Pop();
            _symbolTable.Pop();

            return definition;
        }
        
        public List<Statement> ParseSelectorBodySection(TokenType bodyType)
        {
            List<Statement> body = new List<Statement>();

            _parser.Match(bodyType);

            while (!_parser.Matches(TokenType.Keyword_End))
                body.Add(ParseStatement());

            _parser.Match(TokenType.Keyword_End);

            return body;
        }

        public ProcedureDefinition ParseProcedureDefinition()
        {
            _symbolTable.Push();
            _typeSymbolTable.Push();

            Token reference = _parser.Match(TokenType.Keyword_Function);
            string functionName = _parser.Match(TokenType.Identifier, TokenType.Operator).ProcessedValue;

            List<FunctionParameter> parameters = _parser.ParseDelimitedList(
                TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseFunctionParameter);

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_Return, TokenType.Keyword_End))
                body.Add(ParseStatement());

            ExpressionStatement returnExpression;
            
            if (_parser.Matches(TokenType.Keyword_Return))
            {
                _parser.Next();
                returnExpression = ParseExpression();
                _parser.Match(TokenType.Semicolon);
            }
            else
            {
                returnExpression = new VoidExpressionStatement(reference);
            }

            _parser.Match(TokenType.Keyword_End);

            ProcedureDefinition definition = new ProcedureDefinition(
                reference, functionName, parameters, body, returnExpression);

            _typeSymbolTable.Pop();
            _symbolTable.Pop();

            return definition;
        }

        public FunctionParameter ParseFunctionParameter()
        {
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = CreateEnvironmentVariable(variableNameToken);

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

            if (!_typeSymbolTable.TryGetValue(typeVariableName, out typeVariable))
            {
                typeVariable = new TypeVariable(typeVariableName);
                _typeSymbolTable.Add(typeVariableName, typeVariable);
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

        public Statement ParseStatement()
        {
            switch (_parser.Current.TokenType)
            {
                case TokenType.Keyword_Let:
                    return ParseVariableAssignmentStatement();

                case TokenType.Keyword_Var:
                    return ParseVariableDeclarationStatement();

                case TokenType.Keyword_While:
                    return ParseWhileStatement();

                case TokenType.Keyword_If:
                    return ParseIfStatement();

                case TokenType.OpenBrace:
                    return ParseCommandBlockStatement();

                default:
                    Statement expression = ParseExpression();
                    _parser.Match(TokenType.Semicolon);
                    return expression;
            }
        }

        public Statement ParseVariableDeclarationStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Var);
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            _parser.Match(TokenType.Colon);
            TypeExpressionStatement dataType = ParseTypeExpression();

            Variable variable = CreateEnvironmentVariable(variableNameToken);

            return new VariableDeclarationStatement(reference, variable, dataType);
        }

        public TypeExpressionStatement ParseTypeExpression()
        {
            if (_parser.Matches(TokenType.TypeVariable))
            {
                Token typeVariableNameToken = _parser.Next();
                TypeVariable variable = LookupTypeVariable(typeVariableNameToken);
                return new TypeVariableStatement(typeVariableNameToken, variable);                
            }
            
            return ParseTypeConstructor();
        }

        public TypeConstructorStatement ParseTypeConstructor()
        {
            Token reference;
            string typeName;
            List<TypeExpressionStatement> typeArguments = new List<TypeExpressionStatement>();

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

            List<ExpressionStatement> valueArguments = new List<ExpressionStatement>(0);

            if (_parser.Matches(TokenType.OpenParen))
            {
                valueArguments = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseExpression);
            }

            return new TypeConstructorStatement(reference, typeName, typeArguments, valueArguments);
        }

        public Statement ParseVariableAssignmentStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_Let);

            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = CreateEnvironmentVariable(variableNameToken);

            _parser.Match(TokenType.Colon);

            ExpressionStatement expression = ParseExpression();
            _parser.Match(TokenType.Semicolon);

            return new VariableAssignmentStatement(reference, variable, expression);
        }

        public Statement ParseWhileStatement()
        {
            Token reference = _parser.Match(TokenType.Keyword_While);
            ExpressionStatement condition = ParseExpression();
            _parser.Match(TokenType.Colon);

            _symbolTable.Push();

            List<Statement> body = new List<Statement>();

            while (!_parser.Matches(TokenType.Keyword_End))
                body.Add(ParseStatement());

            _symbolTable.Pop();

            _parser.Match(TokenType.Keyword_End);

            return new WhileStatement(reference, condition, body);
        }

        public Statement ParseIfStatement()
        {
            throw new NotImplementedException();
        }

        public Statement ParseCommandBlockStatement()
        {
            List<Command> commands = new List<Command>();

            Token reference = _parser.Match(TokenType.OpenBrace);

            while (!_parser.Matches(TokenType.CloseBrace))
                commands.Add(ParseCommand());

            _parser.Match(TokenType.CloseBrace);

            return new CommandBlockStatement(reference, commands);
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

            Variable counterVariable = ParseCommandDataItem();
            
            return new RepeatCommand(reference, commands, counterVariable);
        }

        public VariableReferenceCommand ParseVariableReferenceCommand()
        {
            Token variableNameToken = _parser.Match(TokenType.Identifier);
            Variable variable = LookupUserVariable(variableNameToken);

            return new VariableReferenceCommand(variableNameToken, variable);
        }

        public WriteCommand ParseWriteCommand()
        {
            Token reference = _parser.Match(TokenType.Hash);
            List<Variable> variables;

            if (_parser.Matches(TokenType.OpenParen))
            {
                variables = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    ParseCommandDataItem);
            }
            else
            {
                variables = new List<Variable>() { ParseCommandDataItem() };
            }

            return new WriteCommand(reference, variables);
        }
        
        public Variable ParseCommandDataItem()
        {
            if (_parser.Matches(TokenType.Identifier))
            {
                Token variableNameToken = _parser.Next();
                return LookupUserVariable(variableNameToken);
            }
            else
            {
                ExpressionStatement literal = ParseLiteralExpression();
                return literal.ReturnVariable;
            }
        }

        public ExpressionStatement ParseExpression()
        {
            ExpressionStatement left = ParseUnaryExpression();

            if (_parser.Matches(TokenType.Operator))
            {
                Token operatorToken = _parser.Next();

                ExpressionStatement right = ParseExpression();

                left = new FunctionCallExpressionStatement(
                    operatorToken, operatorToken.ProcessedValue, new ExpressionStatement[] { left, right });
            }

            return left;
        }

        public ExpressionStatement ParseUnaryExpression()
        {
            if (_parser.Matches(TokenType.Operator))
            {
                Token operatorToken = _parser.Next();
                ExpressionStatement argument = ParseUnaryExpression();

                return new FunctionCallExpressionStatement(
                    operatorToken, operatorToken.ProcessedValue, new ExpressionStatement[] { argument });
            }

            return ParseAtomicExpression();
        }
        
        public ExpressionStatement ParseAtomicExpression()
        {
            ExpressionStatement returnValue;

            if (_parser.Matches(TokenType.Numeric, TokenType.String))
                returnValue = ParseLiteralExpression();

            else if (_parser.Matches(TokenType.OpenParen))
                returnValue = ParseParenthesizedExpression();

            else if (_parser.MatchesLookahead(TokenType.Identifier, TokenType.OpenParen))
                returnValue = ParseFunctionCallExpression();
            
            else
                returnValue = ParseVariableExpression();

            while (_parser.Matches(TokenType.Period, TokenType.OpenBracket))
                returnValue = ParseExpressionSuffix(returnValue);

            return returnValue;
        }

        public ExpressionStatement ParseExpressionSuffix(ExpressionStatement source)
        {
            if (_parser.Matches(TokenType.OpenBracket))
            {
                Token selectorIndexToken = _parser.Current;

                List<ExpressionStatement> indexArguments = _parser.ParseDelimitedList(
                    TokenType.OpenBracket, TokenType.Comma, TokenType.CloseBracket, ParseExpression);

                return new SelectorIndexExpressionStatement(selectorIndexToken, source, indexArguments);
            }
            else
            {
                _parser.Match(TokenType.Period);
                Token fieldNameToken = _parser.Match(TokenType.Identifier);
                string fieldName = fieldNameToken.ProcessedValue;

                return new FieldExpressionStatement(fieldNameToken, source, fieldName);
            }
        }

        public ExpressionStatement ParseFunctionCallExpression()
        {
            Token nameToken = _parser.Match(TokenType.Identifier);

            List<ExpressionStatement> arguments = _parser.ParseDelimitedList(
                TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen, ParseExpression);
            
            return new FunctionCallExpressionStatement(nameToken, nameToken.ProcessedValue, arguments);
        }

        public ExpressionStatement ParseVariableExpression()
        {
            Token nameToken = _parser.Match(TokenType.Identifier);
            Variable variable = LookupUserVariable(nameToken);
            return new VariableExpressionStatement(nameToken, variable);
        }

        public ExpressionStatement ParseParenthesizedExpression()
        {
            _parser.Match(TokenType.OpenParen);
            ExpressionStatement result = ParseExpression();
            _parser.Match(TokenType.CloseParen);

            return result;
        }

        public ExpressionStatement ParseLiteralExpression()
        {
            Token token = _parser.Match(TokenType.String, TokenType.Numeric);

            if (token.TokenType == TokenType.String)
                return new StringExpressionStatement(token, token.RawValue, token.ProcessedValue);
            else
                return new ConstExpressionStatement(token, token.NumericValue);
        }
    }
}
