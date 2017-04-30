using CyBF.BFC.Model;
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

        public void ParseProgram()
        {
            while (!_parser.Matches(TokenType.EndOfSource))
            {
                ParseStatement();
            }
        }

        public void ParseStatement()
        {
            if (_parser.Matches(TokenType.Keyword_Let))
                ParseVariableAssignmentStatement();

            else if (_parser.Matches(TokenType.Keyword_Var))
                ParseVariableDeclarationStatement();

            else if (_parser.Matches(TokenType.OpenBrace))
                ParseCommandBlockStatement();

            else
                throw new NotImplementedException();    
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
            // Unlike type constructors, type expressions
            // include the possibility of just looking up
            // an existing type variable.

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
            if (_parser.Matches(TokenType.Numeric, TokenType.String))
                return ParseLiteralExpression();

            if (_parser.Matches(TokenType.OpenParen))
                return ParseParenthesizedExpression();

            if (_parser.MatchesLookahead(TokenType.Identifier, TokenType.OpenParen))
                return ParseFunctionCallExpression();

            return ParseVariableExpression();
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
