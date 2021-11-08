using System;

namespace UAlbion.Scripting.Ast
{
    public enum Operation
    {
        Add,
        And,
        Assign,
        Equal,
        Greater,
        GreaterEqual,
        Lesser,
        LesserEqual,
        NotEqual,
        Or,
        Subtract,
    }

    public static class OperationExtensions
    {
        public static string ToPseudocode(this Operation op) => op switch
        {
            Operation.Equal => "==",
            Operation.NotEqual => "!=",
            Operation.GreaterEqual => ">=",
            Operation.LesserEqual => "<=",
            Operation.Greater => ">",
            Operation.Lesser => "<",
            Operation.Add => "+=",
            Operation.Subtract => "-=",
            Operation.Assign => "=",
            Operation.And => "&&",
            Operation.Or => "||",
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}