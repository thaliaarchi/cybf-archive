using CyBF.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyBF.BFIL
{
    public class BFILParser
    {
        Parser _parser;
        Dictionary<TokenType, Func<BFILStatement>> _parseFunctionsByTokenType;

        TokenType[] _commandTokenTypes = new TokenType[]
        {
            TokenType.Plus,
            TokenType.Minus,
            TokenType.OpenAngle,
            TokenType.CloseAngle,
            TokenType.Comma,
            TokenType.Period
        };

        public BFILParser(string code, string source)
        {
            Lexer lexer = new Lexer();

            lexer.SetInput(code, source);
            lexer.Mode = Lexer.LexerMode.Command;
            lexer.LockMode = true;

            List<Token> tokens = lexer.GetAllTokens(true);

            _parser = new Parser(tokens);

            _parseFunctionsByTokenType = new Dictionary<TokenType, Func<BFILStatement>>();
            _parseFunctionsByTokenType[TokenType.At] = this.ParseDeclarationStatement;
            _parseFunctionsByTokenType[TokenType.Identifier] = this.ParseReferenceStatement;
            _parseFunctionsByTokenType[TokenType.Hash] = this.ParseWriteStatement;
            _parseFunctionsByTokenType[TokenType.OpenBracket] = this.ParseLoopStatement;

            foreach (TokenType tt in _commandTokenTypes)
                _parseFunctionsByTokenType[tt] = this.ParseCommandStatement;
        }

        public BFILProgram ParseProgram()
        {
            List<BFILStatement> statements = new List<BFILStatement>();

            while (!_parser.Matches(TokenType.EndOfSource))
                statements.Add(ParseStatement());

            return new BFILProgram(statements);
        }

        public BFILStatement ParseStatement()
        {
            if (!_parseFunctionsByTokenType.ContainsKey(_parser.Current.TokenType))
                throw new SyntaxError(_parser.Current, "BFIL statement");

            return _parseFunctionsByTokenType[_parser.Current.TokenType]();
        }

        public BFILLoopStatement ParseLoopStatement()
        {
            List<BFILStatement> body = new List<BFILStatement>();

            Token reference = _parser.Match(TokenType.OpenBracket);

            while (!_parser.Matches(TokenType.CloseBracket))
                body.Add(ParseStatement());

            _parser.Match(TokenType.CloseBracket);

            return new BFILLoopStatement(reference, body);
        }

        public BFILCommandStatement ParseCommandStatement()
        {
            StringBuilder commandString = new StringBuilder();

            Token reference = _parser.Match(_commandTokenTypes);
            commandString.Append(reference.ProcessedValue);

            while(_parser.Matches(_commandTokenTypes) && 
                reference.PositionInfo.LineNumber == _parser.Current.PositionInfo.LineNumber)
            {
                commandString.Append(_parser.Current.ProcessedValue);
                _parser.Next();
            }

            return new BFILCommandStatement(reference, commandString.ToString());
        }

        public BFILDeclarationStatement ParseDeclarationStatement()
        {
            Token reference = _parser.Match(TokenType.At);
            Token identifier = _parser.Match(TokenType.Identifier);

            if (_parser.Matches(TokenType.Hash))
            {
                _parser.Next();
                List<Token> dataTokens = ParseDataTokens();
                List<byte> bytes = ConvertDataTokensToBytes(dataTokens);
                return new BFILDeclarationStatement(reference, identifier.ProcessedValue, bytes);
            }
            else
            {
                _parser.Match(TokenType.Colon);
                Token size = _parser.Match(TokenType.Numeric);

                if (size.NumericValue <= 0)
                    throw new BFILProgramError(reference, "Invalid declared variable size.");

                return new BFILDeclarationStatement(reference, identifier.ProcessedValue, size.NumericValue);
            }
        }

        public BFILReferenceStatement ParseReferenceStatement()
        {
            Token identifier = _parser.Match(TokenType.Identifier);
            return new BFILReferenceStatement(identifier, identifier.ProcessedValue);
        }

        public BFILWriteStatement ParseWriteStatement()
        {
            Token reference = _parser.Match(TokenType.Hash);
            List<Token> dataTokens = ParseDataTokens();
            List<byte> bytes = ConvertDataTokensToBytes(dataTokens);

            return new BFILWriteStatement(reference, bytes);
        }

        public List<Token> ParseDataTokens()
        {
            List<Token> dataTokens;

            if (_parser.Matches(TokenType.Character, TokenType.String, TokenType.Numeric))
            {
                dataTokens = new List<Token>() { _parser.Next() };
            }
            else
            {
                dataTokens = _parser.ParseDelimitedList(
                    TokenType.OpenParen, TokenType.Comma, TokenType.CloseParen,
                    () => _parser.Match(TokenType.Character, TokenType.String, TokenType.Numeric));
            }

            return dataTokens;
        }

        public List<byte> ConvertDataTokensToBytes(IEnumerable<Token> dataTokens)
        {
            List<byte> data = new List<byte>();

            foreach (Token token in dataTokens)
            {
                if (token.TokenType == TokenType.Character || token.TokenType == TokenType.String)
                {
                    if (token.TokenType == TokenType.String)
                        data.Add(0);

                    try
                    {
                        data.AddRange(Encoding.ASCII.GetBytes(token.ProcessedValue));
                    }
                    catch (EncoderFallbackException)
                    {
                        throw new SyntaxError(token, "ascii encoding");
                    }

                    if (token.TokenType == TokenType.String)
                        data.Add(0);
                }
                else if (token.TokenType == TokenType.Numeric)
                {
                    if (token.NumericValue < 0 || 255 < token.NumericValue)
                        throw new SyntaxError(token, "value within byte range [0-255]");

                    data.Add((byte)token.NumericValue);
                }
                else
                {
                    throw new SyntaxError(token, "data value");
                }
            }

            return data;
        }
    }
}
