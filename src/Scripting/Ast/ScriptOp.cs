using System;

namespace UAlbion.Scripting.Ast;

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
    BitwiseAnd,
    BitwiseOr
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
        ScriptOp.BitwiseAnd => "&",
        ScriptOp.BitwiseOr => "|",
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

        ScriptOp.BitwiseAnd => 5,
        ScriptOp.BitwiseOr => 6,
        ScriptOp.And => 7,
        ScriptOp.Or => 8,

        ScriptOp.Add => 9,
        ScriptOp.Assign => 9,
        ScriptOp.Subtract => 9,
        _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
    };
}