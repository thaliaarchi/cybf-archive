
namespace CyBF.Parsing
{
    public enum TokenType
    {
        Identifier,
        TypeVariable,
        Keyword,

        Numeric,
        Character,
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
        Keyword_Struct,
        Keyword_Function,
        Keyword_Selector,
        Keyword_Reference,
        Keyword_Dereference,
        Keyword_Var,
        Keyword_Let,
        Keyword_Cast,
        Keyword_Sizeof,
        Keyword_String,
        Keyword_New,
        Keyword_Do,
        Keyword_Loop,
        Keyword_While,
        Keyword_For,
        Keyword_Iterate,
        Keyword_Next,
        Keyword_If,
        Keyword_Elif,
        Keyword_Else,
        Keyword_Return,
        Keyword_End,
        
        Whitespace,

        EndOfSource
    }
}
