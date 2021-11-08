using Superpower.Display;

namespace UAlbion.Scripting
{
    public enum ScriptToken
    {
        None,
        Number,
        Identifier,
        NewLine,
        Comment,

        // Grouping
        [Token(Example = "(")] LParen,
        [Token(Example = ")")] RParen,
        [Token(Example = "[")] LBracket,
        [Token(Example = "]")] RBracket,
        [Token(Example = "{")] LBrace,
        [Token(Example = "}")] RBrace,

        [Token(Example = ".")] Dot,
        [Token(Example = ",")] Comma,
        [Token(Example = ":")] Colon,
        [Token(Example = "=")] Assign,

        // Comparison operators
        [Token(Example = "==")] Equal,
        [Token(Example = "!=")] NotEqual,
        [Token(Example = ">=")] GreaterEqual,
        [Token(Example = "<=")] LesserEqual,
        [Token(Example = ">")] Greater,
        [Token(Example = "<")] Lesser,

        // Logical operators
        [Token(Example = "!")] Not,
        [Token(Example = "&")] And,
        [Token(Example = "|")] Or,

        // Arithmetic operators
        [Token(Example = "+=")] Add,
        [Token(Example = "-=")] Sub,
    }
}