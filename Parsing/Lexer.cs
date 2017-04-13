using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CyBF.Parsing
{
    public class Lexer
    {
        private Scanner _scanner;

        private const string _charSubPattern = @"(?:[^""'\\]|\\[0abtnvfr""'\\]|\\x[0-9A-F][0-9A-F])";
        private const string _operatorCharacters = @"!@#$%^&*-+=|\<>?/";
        
        private Regex _identifierRegex = new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*");
        private Regex _decimalRegex = new Regex(@"[1-9][0-9]*");
        private Regex _hexRegex = new Regex(@"0[x][0-9A-F][0-9A-F]");
        private Regex _charRegex = new Regex("\'" + _charSubPattern + "\'");
        private Regex _stringRegex = new Regex("\"" + _charSubPattern + "*\"");
        private Regex _operatorRegex = new Regex(@"[!@#$%^&*-+=|\\<>?/]+");

        private Regex _normalDelimiterRegex = new Regex(@"[;:,.(){}\[\]]");
        private Regex _commandDelimiterRegex = new Regex(@"[{}]");

        private Regex _commandRegex = new Regex(@"(?:[+-\[\]<>,.]|[^\S\n])+");

        private Regex _whitespaceRegex = new Regex(@"\s*(?:`[^\n]*\s*)*");

        private Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            {"var", TokenType.Keyword_Var },
            {"let", TokenType.Keyword_Let },

            {"while", TokenType.Keyword_While },

            {"if", TokenType.Keyword_If },
            {"elif", TokenType.Keyword_Elif },
            {"else", TokenType.Keyword_Else },
            
            {"def", TokenType.Keyword_Def },
            {"returns", TokenType.Keyword_Returns },
            {"end", TokenType.Keyword_End },

            {"type", TokenType.Keyword_Type },

            {"asm", TokenType.Keyword_ASM }
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

        private Dictionary<char, TokenType> _normalDelimiters = new Dictionary<char, TokenType>
        {
            {':', TokenType.Colon },
            {';', TokenType.Semicolon },
            {',', TokenType.Comma },
            {'.', TokenType.Period },
            {'(', TokenType.OpenParen },
            {')', TokenType.CloseParen },
            {'{', TokenType.OpenBrace },
            {'}', TokenType.CloseBrace },
            {'[', TokenType.OpenBracket },
            {']', TokenType.CloseBracket }
        };

        private Dictionary<char, TokenType> _commandDelimiters = new Dictionary<char, TokenType>
        {
            {'{', TokenType.OpenBrace },
            {'}', TokenType.CloseBrace }
        };

        private HashSet<char> _commands = new HashSet<char>("+-[]<>,.");

        private Dictionary<char, Func<Token>> _nextTokenizer;
        private Dictionary<char, Func<Token>> _nextCommandTokenizer;

        public enum LexerMode { Normal, Command }
        public LexerMode Mode { get; set; }

        public Lexer(string sourceName, string source)
        {
            _scanner = new Scanner(sourceName, source);

            _nextTokenizer = new Dictionary<char, Func<Token>>();
            _nextCommandTokenizer = new Dictionary<char, Func<Token>>();

            for (char c = 'a'; c <= 'z'; c++)
            {
                _nextTokenizer[c] = IdentifierOrKeyword;
                _nextCommandTokenizer[c] = IdentifierOrKeyword;
            }

            for (char c = 'A'; c <= 'Z'; c++)
            {
                _nextTokenizer[c] = IdentifierOrKeyword;
                _nextCommandTokenizer[c] = IdentifierOrKeyword;
            }

            _nextTokenizer['_'] = IdentifierOrKeyword;
            _nextCommandTokenizer['_'] = IdentifierOrKeyword;

            for (char c = '1'; c <= '9'; c++)
                _nextTokenizer[c] = DecimalNumeric;

            _nextTokenizer['0'] = HexNumeric;
            _nextTokenizer['\''] = CharacterLiteral;
            _nextTokenizer['\"'] = StringLiteral;

            foreach (char c in _operatorCharacters)
                _nextTokenizer[c] = Operator;

            foreach (char c in _normalDelimiters.Keys)
                _nextTokenizer[c] = NormalDelimiter;

            foreach (char c in _commandDelimiters.Keys)
                _nextCommandTokenizer[c] = CommandDelimiter;

            foreach (char c in _commands)
                _nextCommandTokenizer[c] = CommandString;

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

            this.Mode = LexerMode.Normal;
        }

        public List<Token> GetAllTokens(bool removeWhitespace = true)
        {
            List<Token> tokens = new List<Token>();

            Token t = this.Next();

            while (t != null)
            {
                if (t.TokenType != TokenType.Whitespace || !removeWhitespace)
                    tokens.Add(t);

                if (t.TokenType == TokenType.Keyword_ASM)
                    this.Mode = LexerMode.Command;

                if (this.Mode == LexerMode.Command && t.TokenType == TokenType.CloseBrace)
                    this.Mode = LexerMode.Normal;

                t = this.Next();
            }

            tokens.Add(new Token(_scanner.GetPositionInfo(), TokenType.EndOfSource, string.Empty, 0));

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

        public Token DecimalNumeric()
        {
            return ParsePattern(
                "decimal numeric", 
                _decimalRegex, 
                value => value,
                value => TokenType.Numeric, 
                value => int.Parse(value));
        }

        public Token HexNumeric()
        {
            return ParsePattern(
                "hexadecimal numeric",
                _hexRegex,
                value => value,
                value => TokenType.Numeric,
                value => int.Parse(value.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier));
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

        public Token NormalDelimiter()
        {
            return ParsePattern(
                "delimiter",
                _normalDelimiterRegex,
                value => value,
                value => _normalDelimiters[value[0]],
                value => 0);
        }

        public Token CommandDelimiter()
        {
            return ParsePattern(
                "command delimiter",
                _commandDelimiterRegex,
                value => value,
                value => _commandDelimiters[value[0]],
                value => 0);
        }

        public Token CommandString()
        {
            return ParsePattern(
                "command",
                _commandRegex,
                RemoveWhitespace,
                value => TokenType.CommandString,
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
                if (i != '\\')
                {
                    builder.Append(value[i]);
                    continue;
                }

                i++;

                if (value[i] == 'x')
                {
                    string hexcode = value.Substring(i + 1, 3);
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

            string value;

            if (_scanner.ReadPattern(pattern, out value))
            {
                value = processValue(value);
                return new Token(currentPosition, getTokenType(value), value, getNumericValue(value));
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
                    _scanner.SourceName + "\n" + 
                    "Unexpected end of source found.\n" +
                    "Expected " + expected;
            }
            else
            {
                message =
                    _scanner.SourceName + "\n" +
                    "Line " + _scanner.LineNumber.ToString() + "\n" +
                    _scanner.Line + "\n" +
                    new string(' ', _scanner.LinePosition) + "^\n" +
                    "Expected " + expected;
            }

            throw new LexicalError(message);
        }
    }
}
