﻿
namespace CyBF.Parsing
{
    public enum TokenType
    {
        Identifier,
        Keyword,

        Numeric,
        String,

        Operator,

        Colon,
        Semicolon,
        Comma,
        Period,
        OpenParen,
        CloseParen,
        OpenBrace,
        CloseBrace,
        OpenBracket,
        CloseBracket,

        Plus,
        Minus,
        OpenAngle,
        CloseAngle,
        At,
        Hash,
        Asterisk,

        Keyword_Module,
        Keyword_Import,
        Keyword_Struct,
        Keyword_Def,
        Keyword_Var,
        Keyword_Let,
        Keyword_While,
        Keyword_Iterate,
        Keyword_If,
        Keyword_Elif,
        Keyword_Else,
        Keyword_Return,
        Keyword_End,
        
        Whitespace,

        EndOfSource
    }
}
