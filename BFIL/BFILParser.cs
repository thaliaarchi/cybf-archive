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
            Lexer lexer = new Lexer(code, source);

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
            commandString.Append(reference.Value);

            while(_parser.Matches(_commandTokenTypes) && 
                reference.PositionInfo.LineNumber == _parser.Current.PositionInfo.LineNumber)
            {
                commandString.Append(_parser.Current.Value);
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
                return new BFILDeclarationStatement(reference, identifier.Value, bytes);
            }
            else
            {
                _parser.Match(TokenType.Colon);
                Token size = _parser.Match(TokenType.Numeric);

                if (size.NumericValue <= 0)
                    throw new BFILProgramError(reference, "Invalid declared variable size.");

                return new BFILDeclarationStatement(reference, identifier.Value, size.NumericValue);
            }
        }

        public BFILReferenceStatement ParseReferenceStatement()
        {
            Token identifier = _parser.Match(TokenType.Identifier);
            return new BFILReferenceStatement(identifier, identifier.Value);
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
            List<Token> dataTokens = new List<Token>();

            if (_parser.Matches(TokenType.Numeric, TokenType.String))
            {
                dataTokens.Add(_parser.Next());
            }
            else
            {
                _parser.Match(TokenType.OpenParen);
                dataTokens.Add(_parser.Match(TokenType.Numeric, TokenType.String));

                while (_parser.Matches(TokenType.Comma))
                {
                    _parser.Next();
                    dataTokens.Add(_parser.Match(TokenType.Numeric, TokenType.String));
                }

                _parser.Match(TokenType.CloseParen);
            }

            return dataTokens;
        }

        public List<byte> ConvertDataTokensToBytes(IEnumerable<Token> dataTokens)
        {
            List<byte> data = new List<byte>();

            foreach (Token token in dataTokens)
            {
                if (token.TokenType == TokenType.Numeric)
                {
                    if (token.NumericValue < 0 || 255 < token.NumericValue)
                        throw new SyntaxError(token, "value within byte range [0-255]");

                    data.Add((byte)token.NumericValue);
                }
                else if (token.TokenType == TokenType.String)
                {
                    data.Add(0);

                    try
                    {
                        data.AddRange(Encoding.UTF8.GetBytes(token.Value));
                    }
                    catch (EncoderFallbackException)
                    {
                        throw new SyntaxError(token, "utf-8 encoding");
                    }

                    data.Add(0);
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
