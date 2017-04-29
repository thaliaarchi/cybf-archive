using CyBF.BFC.Model;
using CyBF.BFC.Model.Statements;
using CyBF.BFC.Model.Statements.Commands;
using CyBF.Parsing;
using System.Collections.Generic;
using System.Text;

namespace CyBF.BFC.Compilation
{
    public class ModelBuilder
    {
        private Parser _parser;
        private DefinitionLibrary _library;
        private Environment _environment;
        private int _variableAutonum;
        
        public ModelBuilder(IEnumerable<Token> programTokens)
        {
            _parser = new Parser(programTokens);
            _library = new DefinitionLibrary();
            _environment = new Environment();
            _variableAutonum = 1;
        }

        public string Compile()
        {
            BFCompiler compiler = new BFCompiler(_library);

            foreach (Statement statement in _environment.CurrentStatements)
                statement.Compile(compiler);

            return compiler.GetCode();
        }

        public Variable NewVariable()
        {
            return new Variable("var" + (_variableAutonum++).ToString());
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
                ParseCommandBlockStatement();
            }
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

            Variable counterVariable;

            if (_parser.Matches(TokenType.Identifier))
                counterVariable = LookupVariable(_parser.Next());
            else
                counterVariable = ParseLiteralStatement();

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
                LookupVariable(_parser.Next()) : ParseLiteralStatement();

            variables.Add(variable);

            if (writeList)
            {
                while (_parser.Matches(TokenType.Comma))
                {
                    _parser.Next();

                    variable = _parser.Matches(TokenType.Identifier) ?
                        LookupVariable(_parser.Next()) : ParseLiteralStatement();

                    variables.Add(variable);
                }

                _parser.Match(TokenType.CloseParen);
            }

            return new WriteCommand(reference, variables);
        }
        
        public Variable ParseLiteralStatement()
        {
            Token reference = _parser.Match(TokenType.String, TokenType.Numeric);
            Variable returnValue = NewVariable();
            Statement statement;

            if (reference.TokenType == TokenType.String)
                statement = new StringStatement(reference, new CyBFString(reference.RawValue, reference.ProcessedValue), returnValue);
            else
                statement = new ConstStatement(reference, reference.NumericValue, returnValue);

            _environment.Append(statement);

            return returnValue;

        }
    }
}
