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
            Token variable = _parser.Match(TokenType.Identifier);
            _parser.Match(TokenType.Colon);
            Token size = _parser.Match(TokenType.Numeric);

            return new BFILDeclarationStatement(reference, variable.Value, size.NumericValue);
        }

        public BFILReferenceStatement ParseReferenceStatement()
        {
            Token token = _parser.Match(TokenType.Identifier);
            return new BFILReferenceStatement(token, token.Value);
        }

        public BFILWriteStatement ParseWriteStatement()
        {
            _parser.Match(TokenType.Hash);

            if (_parser.Matches(TokenType.Numeric))
            {
                Token token = _parser.Next();
                return new BFILWriteStatement(token, ConvertNumericToken(token));
            }
            else if (_parser.Matches(TokenType.String))
            {
                Token token = _parser.Next();
                return new BFILWriteStatement(token, ConvertStringToken(token));
            }
            else
            {
                Token referenceToken = _parser.Match(TokenType.OpenParen);

                List<byte> bytes = new List<byte>();

                while (!_parser.Matches(TokenType.CloseParen))
                {
                    Token token = _parser.Match(TokenType.Numeric, TokenType.String);

                    if (token.TokenType == TokenType.Numeric)
                        bytes.AddRange(ConvertNumericToken(token));
                    else
                        bytes.AddRange(ConvertStringToken(token));
                }

                _parser.Match(TokenType.CloseParen);

                return new BFILWriteStatement(referenceToken, bytes);
            }
        }

        private byte[] ConvertNumericToken(Token token)
        {
            if (token.TokenType != TokenType.Numeric)
                throw new SyntaxError(token, "numeric");

            int value = token.NumericValue;

            if (value < 0 || 255 < value)
                throw new SyntaxError(token, "value within byte range [0-255]");

            return new byte[] { (byte)value };
        }

        private byte[] ConvertStringToken(Token token)
        {
            if (token.TokenType != TokenType.String)
                throw new SyntaxError(token, "string");

            string str = token.Value;
            byte[] bytes;

            try
            {
                int strlen = str.Length;
                int bytelen = 2 + Encoding.UTF8.GetByteCount(str);

                bytes = new byte[bytelen];
                bytes[0] = 0;
                bytes[bytelen - 1] = 0;

                Encoding.UTF8.GetBytes(str, 0, strlen, bytes, 1);
            }
            catch (EncoderFallbackException)
            {
                throw new SyntaxError(token, "utf-8 encoding");
            }

            return bytes;
        }
    }
}
