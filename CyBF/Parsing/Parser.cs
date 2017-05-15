using System;
using System.Collections.Generic;
using System.Linq;

namespace CyBF.Parsing
{
    public class Parser
    {
        private List<Token> _tokens;
        private int _currentIndex = 0;

        public Token Current { get { return _tokens[_currentIndex]; } }

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList();

            if (_tokens.Count == 0)
                throw new ArgumentException("Tokens list is empty.");

            if (_tokens.Last().TokenType != TokenType.EndOfSource)
                throw new ArgumentException("Tokens list does not include terminating EndOfSource token.");
        }

        public Token Next()
        {
            Token token = _tokens[_currentIndex];

            if (_currentIndex < _tokens.Count - 1)
                _currentIndex++;

            return token;
        }

        public Token Match(params TokenType[] types)
        {
            Token token = this.Next();

            if (!types.Contains(token.TokenType))
                throw new SyntaxError(token, string.Join(", ", types.Select(t => t.ToString())));

            return token;
        }

        public bool Matches(params TokenType[] types)
        {
            Token token = this.Current;

            return types.Contains(token.TokenType);
        }

        public bool MatchesLookahead(params TokenType[] types)
        {
            if (types.Length > _tokens.Count - _currentIndex)
                return false;

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] != _tokens[_currentIndex + i].TokenType)
                    return false;
            }

            return true;
        }

        public List<T> ParseDelimitedList<T>(TokenType startToken, TokenType delimiter, TokenType endToken, Func<T> parsefn)
        {
            List<T> result = new List<T>();

            this.Match(startToken);

            if (!this.Matches(endToken))
            {
                result.Add(parsefn());

                while (this.Matches(delimiter))
                {
                    this.Next();
                    result.Add(parsefn());
                }
            }

            this.Match(endToken);

            return result;
        }
    }
}
