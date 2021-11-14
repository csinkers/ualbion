using UAlbion.Api;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class Emit
    {
        public static AlbionEvent Event(IEvent e) => new(e);
        public static BreakStatement Break() => new();
        public static ContinueStatement Continue() => new();
        public static DoLoop Do(ICfgNode condition, ICfgNode body) => new(condition, body);
        public static EmptyNode Empty() => new();
        public static IfThen If(ICfgNode condition, ICfgNode body) => new(condition, body);
        public static IfThenElse IfElse(ICfgNode condition, ICfgNode body, ICfgNode elseBody) => new(condition, body, elseBody);
        public static Goto Goto(string label) => new(label);
        public static Label Label(string name) => new(name);
        public static Name Name(string name) => new(name);
        public static Negation Negation(ICfgNode expression) => new(expression);
        public static Numeric Const(int num) => new(num);
        public static Sequence Seq(params ICfgNode[] statements) => new(statements);
        public static Statement Statement(ICfgNode head, params ICfgNode[] parameters) => new(head, parameters);
        public static WhileLoop While(ICfgNode condition, ICfgNode body) => new(condition, body);

        public static BinaryOp Op(ScriptOp operation, ICfgNode left, ICfgNode right) => new(operation, left, right);
        public static BinaryOp Add(ICfgNode parent, ICfgNode child) => new(ScriptOp.Add, parent, child);
        public static BinaryOp And(ICfgNode parent, ICfgNode child) => new(ScriptOp.And, parent, child);
        public static BinaryOp Assign(ICfgNode parent, ICfgNode child) => new(ScriptOp.Assign, parent, child);
        public static BinaryOp Eq(ICfgNode parent, ICfgNode child) => new(ScriptOp.Equal, parent, child);
        public static BinaryOp Gt(ICfgNode parent, ICfgNode child) => new(ScriptOp.Greater, parent, child);
        public static BinaryOp Gte(ICfgNode parent, ICfgNode child) => new(ScriptOp.GreaterEqual, parent, child);
        public static BinaryOp Lt(ICfgNode parent, ICfgNode child) => new(ScriptOp.Lesser, parent, child);
        public static BinaryOp Lte(ICfgNode parent, ICfgNode child) => new(ScriptOp.LesserEqual, parent, child);
        public static BinaryOp Member(ICfgNode parent, ICfgNode child) => new(ScriptOp.Member, parent, child);
        public static BinaryOp Neq(ICfgNode parent, ICfgNode child) => new(ScriptOp.NotEqual, parent, child);
        public static BinaryOp Or(ICfgNode parent, ICfgNode child) => new(ScriptOp.Or, parent, child);
        public static BinaryOp Sub(ICfgNode parent, ICfgNode child) => new(ScriptOp.Subtract, parent, child);
    }
}