using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CyBF.Parsing
{
    public class Lexer
    {
        private Scanner _scanner;

        private const string _charSubPattern = @"(?:[^'\\]|\\[0abtnvfr""'\\]|\\x[0-9A-F][0-9A-F])";
        private const string _stringSubPattern = @"(?:[^""\\]|\\[0abtnvfr""'\\]|\\x[0-9A-F][0-9A-F])";
        private const string _operatorCharacters = @"!@#$%^&*-+=|\<>?/";
        private const string _commandCharacters = @"+-[]<>,.@:#()*{}";

        private Regex _identifierRegex = new Regex(@"\G[a-zA-Z_][a-zA-Z0-9_]*");
        private Regex _typevarRegex = new Regex(@"\G~[a-zA-Z_][a-zA-Z0-9_]*");
        private Regex _decimalRegex = new Regex(@"\G[0-9]+");
        private Regex _charRegex = new Regex(@"\G'" + _charSubPattern + @"'");
        private Regex _stringRegex = new Regex(@"\G""" + _stringSubPattern + @"*""");
        private Regex _operatorRegex = new Regex(@"\G[!@#$%^&*\-+=|\\<>?/]+");
        private Regex _commandRegex = new Regex(@"\G[+-\[\]<>,.@:#()*{}]");
        private Regex _delimiterRegex = new Regex(@"\G[;:,.(){}\[\]]");
        
        private Regex _whitespaceRegex = new Regex(@"\G\s*(?:`[^\n]*\s*)*");

        private Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            {"module", TokenType.Keyword_Module },
            {"struct", TokenType.Keyword_Struct },
            {"function", TokenType.Keyword_Function },
            {"selector", TokenType.Keyword_Selector },
            {"reference", TokenType.Keyword_Reference },
            {"dereference", TokenType.Keyword_Dereference },
            {"var", TokenType.Keyword_Var },
            {"let", TokenType.Keyword_Let },
            {"cast", TokenType.Keyword_Cast },
            {"sizeof", TokenType.Keyword_Sizeof },
            {"string", TokenType.Keyword_String },
            {"new", TokenType.Keyword_New },
            {"do", TokenType.Keyword_Do },
            {"while", TokenType.Keyword_While },
            {"for", TokenType.Keyword_For },
            {"iterate", TokenType.Keyword_Iterate },
            {"if", TokenType.Keyword_If },
            {"elif", TokenType.Keyword_Elif },
            {"else", TokenType.Keyword_Else },
            {"return", TokenType.Keyword_Return },
            {"end", TokenType.Keyword_End }
        };

        private Dictionary<char, char> _escapeCodes = new Dictionary<char, char>
        {
            {'0', '\0'},
            {'a', '\a'},
            {'b', '\b'},
            {'t', '\t'},
            {'n', '\n'},
            {'v', '\v'},
            {'f', '\f'},
            {'r', '\r'}
        };

        private Dictionary<char, TokenType> _delimiters = new Dictionary<char, TokenType>
        {
            {';', TokenType.Semicolon },
            {':', TokenType.Colon },
            {',', TokenType.Comma },
            {'.', TokenType.Period },
            {'(', TokenType.OpenParen },
            {')', TokenType.CloseParen },
            {'{', TokenType.OpenBrace },
            {'}', TokenType.CloseBrace },
            {'[', TokenType.OpenBracket },
            {']', TokenType.CloseBracket }
        };

        private Dictionary<char, TokenType> _commands = new Dictionary<char, TokenType>
        {
            {'+', TokenType.Plus },
            {'-', TokenType.Minus },
            {'[', TokenType.OpenBracket },
            {']', TokenType.CloseBracket },
            {'<', TokenType.OpenAngle },
            {'>', TokenType.CloseAngle },
            {',', TokenType.Comma },
            {'.', TokenType.Period },
            {'@', TokenType.At },
            {':', TokenType.Colon },
            {'#', TokenType.Hash },
            {'(', TokenType.OpenParen },
            {')', TokenType.CloseParen },
            {'*', TokenType.Asterisk },
            {'{', TokenType.OpenBrace },
            {'}', TokenType.CloseBrace }
        };

        private Dictionary<char, Func<Token>> _nextTokenizer;
        private Dictionary<char, Func<Token>> _nextCommandTokenizer;

        public enum LexerMode { Normal, Command }

        public bool LockMode { get; set; }

        private LexerMode _mode;
        public LexerMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (this.LockMode)
                    throw new InvalidOperationException();

                _mode = value;
            }
        }

        public Lexer()
        {
            _scanner = new Scanner(string.Empty, string.Empty);

            _nextTokenizer = new Dictionary<char, Func<Token>>();
            _nextCommandTokenizer = new Dictionary<char, Func<Token>>();

            for (char c = 'a'; c <= 'z'; c++)
            {
                _nextTokenizer[c] = IdentifierOrKeyword;
                _nextCommandTokenizer[c] = IdentifierOrKeyword;
            }

            _nextTokenizer['~'] = TypeVariable;

            for (char c = 'A'; c <= 'Z'; c++)
            {
                _nextTokenizer[c] = IdentifierOrKeyword;
                _nextCommandTokenizer[c] = IdentifierOrKeyword;
            }

            _nextTokenizer['_'] = IdentifierOrKeyword;
            _nextCommandTokenizer['_'] = IdentifierOrKeyword;

            for (char c = '0'; c <= '9'; c++)
            {
                _nextTokenizer[c] = DecimalNumeric;
                _nextCommandTokenizer[c] = DecimalNumeric;
            }
            
            _nextTokenizer['\''] = CharacterLiteral;
            _nextCommandTokenizer['\''] = CharacterLiteral;

            _nextTokenizer['\"'] = StringLiteral;
            _nextCommandTokenizer['\"'] = StringLiteral;

            foreach (char c in _operatorCharacters)
                _nextTokenizer[c] = Operator;

            foreach (char c in _delimiters.Keys)
                _nextTokenizer[c] = Delimiter;

            foreach (char c in _commands.Keys)
                _nextCommandTokenizer[c] = Command;

            _nextTokenizer['`'] = Whitespace;
            _nextCommandTokenizer['`'] = Whitespace;

            for (int b = 0; b < 256; b++)
            {
                char c = Encoding.ASCII.GetChars(new byte[] { (byte)b })[0];

                if (Char.IsWhiteSpace(c))
                {
                    _nextTokenizer[c] = Whitespace;
                    _nextCommandTokenizer[c] = Whitespace;
                }
            }

            this.LockMode = false;
            this.Mode = LexerMode.Normal;
        }

        public void SetInput(string code, string source)
        {
            _scanner = new Scanner(code, source);
        }

        public List<Token> GetAllTokens(bool removeWhitespace = true)
        {
            List<Token> tokens = new List<Token>();

            Token t = this.Next();

            while (t != null)
            {
                if (t.TokenType != TokenType.Whitespace || !removeWhitespace)
                    tokens.Add(t);

                if (t.TokenType == TokenType.OpenBrace && !this.LockMode)
                    this.Mode = LexerMode.Command;

                if (t.TokenType == TokenType.CloseBrace && !this.LockMode)
                    this.Mode = LexerMode.Normal;

                t = this.Next();
            }

            tokens.Add(new Token(_scanner.GetPositionInfo(), TokenType.EndOfSource, string.Empty));

            return tokens;
        }

        public Token Next()
        {
            if (!_scanner.EndOfSource)
            {
                char c = (char)_scanner.CurrentValue;

                if (this.Mode == LexerMode.Normal)
                {
                    if (_nextTokenizer.ContainsKey(c))
                        return _nextTokenizer[c]();
                    else
                        RaiseLexicalError("normal mode token");
                }
                else
                {
                    if (_nextCommandTokenizer.ContainsKey(c))
                        return _nextCommandTokenizer[c]();
                    else
                        RaiseLexicalError("command mode token");
                }
            }

            return null;
        }

        public Token IdentifierOrKeyword()
        {
            return ParsePattern(
                "identifier/keyword",
                _identifierRegex,
                value => _keywords.ContainsKey(value) ? _keywords[value] : TokenType.Identifier);
        }

        public Token TypeVariable()
        {
            return ParsePattern(
                "type variable",
                _typevarRegex,
                value => TokenType.TypeVariable);
        }

        public Token DecimalNumeric()
        {
            Token token = ParsePattern(
                "decimal numeric", 
                _decimalRegex, 
                value => TokenType.Numeric);

            token.NumericValue = int.Parse(token.TokenString);

            return token;
        }
        
        public Token CharacterLiteral()
        {
            Token token = ParsePattern(
                "character literal",
                _charRegex,
                value => TokenType.Character);

            string processedString = ProcessEscapedStringLiteral(token.TokenString);
            byte[] asciiBytes = null;

            try
            {
                asciiBytes = Encoding.ASCII.GetBytes(processedString);
            }
            catch(EncoderFallbackException)
            {
                RaiseLexicalError("Character literal not ASCII compatible.", token);
            }

            if (asciiBytes.Length != 1)
                RaiseLexicalError("Unexpected byte count of ASCII decoded character literal.", token);

            token.NumericValue = asciiBytes[0];
            token.AsciiBytes = asciiBytes;

            return token;
        }

        public Token StringLiteral()
        {
            Token token = ParsePattern(
                "string literal",
                _stringRegex,
                value => TokenType.String);

            string processedString = ProcessEscapedStringLiteral(token.TokenString);
            byte[] asciiBytes = null;

            try
            {
                asciiBytes = Encoding.ASCII.GetBytes(processedString);
            }
            catch (EncoderFallbackException)
            {
                RaiseLexicalError("Character literal not ASCII compatible.", token);
            }

            token.AsciiBytes = asciiBytes;

            return token;
        }

        public Token Operator()
        {
            return ParsePattern(
                "operator",
                _operatorRegex,
                value => TokenType.Operator);
        }

        public Token Delimiter()
        {
            return ParsePattern(
                "delimiter",
                _delimiterRegex,
                value => _delimiters[value[0]]);
        }

        public Token Command()
        {
            return ParsePattern(
                "command",
                _commandRegex,
                value => _commands[value[0]]);
        }

        public Token Whitespace()
        {
            return ParsePattern(
                "whitespace",
                _whitespaceRegex,
                value => TokenType.Whitespace);
        }
        
        public string ProcessEscapedStringLiteral(string value)
        {
            StringBuilder builder = new StringBuilder(value.Length * 2);

            for (int i = 1; i < value.Length - 1; i++)
            {
                if (value[i] != '\\')
                {
                    builder.Append(value[i]);
                    continue;
                }

                i++;

                if (value[i] == 'x')
                {
                    string hexcode = value.Substring(i + 1, 2);
                    byte byteValue = byte.Parse(hexcode, System.Globalization.NumberStyles.AllowHexSpecifier);
                    builder.Append(Encoding.ASCII.GetChars(new byte[] { byteValue })[0]);

                    i += 2;
                }
                else if (_escapeCodes.ContainsKey(value[i]))
                {
                    builder.Append(_escapeCodes[value[i]]);
                }
                else
                {
                    builder.Append(value[i]);
                }
            }

            return builder.ToString();
        }

        private Token ParsePattern(string expected, Regex pattern, Func<string, TokenType> getTokenType)
        {
            PositionInfo currentPosition = _scanner.GetPositionInfo();

            string tokenString;

            if (_scanner.ReadPattern(pattern, out tokenString))
            {
                return new Token(currentPosition, getTokenType(tokenString), tokenString);
            }
            else
            {
                RaiseLexicalError(expected);
                return null;
            }
        }
        
        private void RaiseLexicalError(string expected)
        {
            string message;

            if (_scanner.EndOfSource)
            {
                message =
                    _scanner.Source + "\n" + 
                    "Unexpected end of source found.\n" +
                    "Expected " + expected;
            }
            else
            {
                message = _scanner.Source + "\n" 
                    + "Line " + _scanner.LineNumber.ToString() + "\n" 
                    + _scanner.Line + "\n" 
                    + new string(' ', _scanner.LinePosition) + "^\n" 
                    + "Expected " + expected;
            }

            throw new LexicalError(message);
        }

        private void RaiseLexicalError(string message, Token token)
        {
            PositionInfo pos = token.PositionInfo;

            message = pos.Source + "\n"
                + "Line " + pos.LineNumber.ToString() + "\n"
                + pos.Line + "\n"
                + new string(' ', pos.LinePosition) + "^\n"
                + message;

            throw new LexicalError(message);
        }
    }
}
