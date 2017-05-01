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
        private Regex _operatorRegex = new Regex(@"\G[!@#$%^&*-+=|\\<>?/]+");
        private Regex _commandRegex = new Regex(@"\G[+-\[\]<>,.@:#()*{}]");
        private Regex _delimiterRegex = new Regex(@"\G[;:,.(){}\[\]]");
        
        private Regex _whitespaceRegex = new Regex(@"\G\s*(?:`[^\n]*\s*)*");

        private Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            {"module", TokenType.Keyword_Module },
            {"struct", TokenType.Keyword_Struct },
            {"function", TokenType.Keyword_Function },
            {"reference", TokenType.Keyword_Reference },
            {"dereference", TokenType.Keyword_Dereference },
            {"var", TokenType.Keyword_Var },
            {"let", TokenType.Keyword_Let },
            {"while", TokenType.Keyword_While },
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

            tokens.Add(new Token(_scanner.GetPositionInfo(), TokenType.EndOfSource, string.Empty, string.Empty, 0));

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
                value => value,
                value => _keywords.ContainsKey(value) ? _keywords[value] : TokenType.Identifier,
                value => 0);
        }

        public Token TypeVariable()
        {
            return ParsePattern(
                "type variable",
                _typevarRegex,
                value => value,
                value => TokenType.TypeVariable,
                value => 0);
        }

        public Token DecimalNumeric()
        {
            return ParsePattern(
                "decimal numeric", 
                _decimalRegex, 
                value => value,
                value => TokenType.Numeric, 
                value => int.Parse(value));
        }
        
        public Token CharacterLiteral()
        {
            return ParsePattern(
                "character literal",
                _charRegex,
                ProcessString,
                value => TokenType.Numeric,
                value => Encoding.ASCII.GetBytes(value)[0]);
        }

        public Token StringLiteral()
        {
            return ParsePattern(
                "string literal",
                _stringRegex,
                ProcessString,
                value => TokenType.String,
                value => 0);
        }

        public Token Operator()
        {
            return ParsePattern(
                "operator",
                _operatorRegex,
                value => value,
                value => TokenType.Operator,
                value => 0);
        }

        public Token Delimiter()
        {
            return ParsePattern(
                "delimiter",
                _delimiterRegex,
                value => value,
                value => _delimiters[value[0]],
                value => 0);
        }

        public Token Command()
        {
            return ParsePattern(
                "command",
                _commandRegex,
                value => value,
                value => _commands[value[0]],
                value => 0);
        }

        public Token Whitespace()
        {
            return ParsePattern(
                "whitespace",
                _whitespaceRegex,
                value => value,
                value => TokenType.Whitespace,
                value => 0);
        }

        private string RemoveWhitespace(string value)
        {
            StringBuilder builder = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (!Char.IsWhiteSpace(c))
                    builder.Append(c);
            }

            return builder.ToString();
        }

        private string ProcessString(string value)
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

        private Token ParsePattern(string expected, Regex pattern, Func<string, string> processValue, Func<string, TokenType> getTokenType, Func<string, int> getNumericValue)
        {
            PositionInfo currentPosition = _scanner.GetPositionInfo();

            string rawValue;

            if (_scanner.ReadPattern(pattern, out rawValue))
            {
                string processedValue = processValue(rawValue);
                return new Token(currentPosition, getTokenType(processedValue), rawValue, processedValue, getNumericValue(processedValue));
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
                message =
                    _scanner.Source + "\n" +
                    "Line " + _scanner.LineNumber.ToString() + "\n" +
                    _scanner.Line + "\n" +
                    new string(' ', _scanner.LinePosition) + "^\n" +
                    "Expected " + expected;
            }

            throw new LexicalError(message);
        }
    }
}
