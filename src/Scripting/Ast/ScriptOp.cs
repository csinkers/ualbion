using System;

namespace UAlbion.Scripting.Ast
{
    public enum ScriptOp
    {
        Add,
        And,
        Assign,
        Equal,
        Greater,
        GreaterEqual,
        Lesser,
        LesserEqual,
        Member,
        NotEqual,
        Or,
        Subtract,
    }

    public static class OperationExtensions
    {
        public static string ToPseudocode(this ScriptOp op) => op switch
        {
            ScriptOp.Add => "+=",
            ScriptOp.And => "&&",
            ScriptOp.Assign => "=",
            ScriptOp.Equal => "==",
            ScriptOp.Greater => ">",
            ScriptOp.GreaterEqual => ">=",
            ScriptOp.Lesser => "<",
            ScriptOp.LesserEqual => "<=",
            ScriptOp.Member => ".",
            ScriptOp.NotEqual => "!=",
            ScriptOp.Or => "||",
            ScriptOp.Subtract => "-=",
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };

        public static int Priority(this ScriptOp op) => op switch
        { // Parens = 0; Member = 1; Negation = 2
            ScriptOp.Member => 1,

            ScriptOp.Greater => 3,
            ScriptOp.GreaterEqual => 3,
            ScriptOp.Lesser => 3,
            ScriptOp.LesserEqual => 3,

            ScriptOp.Equal => 4,
            ScriptOp.NotEqual => 4,

            ScriptOp.And => 5,
            ScriptOp.Or => 6,

            ScriptOp.Add => 7,
            ScriptOp.Assign => 7,
            ScriptOp.Subtract => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}