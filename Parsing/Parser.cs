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
    }
}
