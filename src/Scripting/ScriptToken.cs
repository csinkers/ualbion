using Superpower.Display;

namespace UAlbion.Scripting
{
    public enum ScriptToken
    {
        Number,
        Identifier,
        NewLine,
        Comment,
        [Token(Example = ".")] Dot,
        [Token(Example = ":")] Colon,
        [Token(Example = "{")] LBrace,
        [Token(Example = "}")] RBrace,
        [Token(Example = "(")] LParen,
        [Token(Example = ")")] Rparen,
        [Token(Example = "=")] Assign,

        [Token(Example = "==")] Equal,
        [Token(Example = "!=")] NotEqual,
        [Token(Example = ">=")] GreaterEqual,
        [Token(Example = "<=")] LesserEqual,
        [Token(Example = ">")] Greater,
        [Token(Example = "<")] Lesser,

        [Token(Example = "!")] Not,
        [Token(Example = "&")] And,
        [Token(Example = "|")] Or,
    }
}