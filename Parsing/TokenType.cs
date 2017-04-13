
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

        Keyword_Var,
        Keyword_Let,

        Keyword_While,

        Keyword_If,
        Keyword_Elif,
        Keyword_Else,

        Keyword_Def,
        Keyword_Returns,
        Keyword_End,

        Keyword_Type,

        Keyword_ASM,

        CommandString,

        Whitespace,

        EndOfSource
    }
}
