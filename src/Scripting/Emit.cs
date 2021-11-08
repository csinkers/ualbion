using UAlbion.Api;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class Emit
    {
        public static AlbionEvent Event(IEvent e) => new(e);
        public static BinaryOp Op(Operation operation, ICfgNode left, ICfgNode right) => new(operation, left, right);
        public static BreakStatement Break() => new();
        public static ContinueStatement Continue() => new();
        public static DoLoop Do(ICfgNode condition, ICfgNode body) => new(condition, body);
        public static EmptyNode Empty() => new();
        public static IfThen If(ICfgNode condition, ICfgNode body) => new(condition, body);
        public static IfThenElse IfElse(ICfgNode condition, ICfgNode body, ICfgNode elseBody) => new(condition, body, elseBody);
        public static Indexed Index(ICfgNode parent, ICfgNode child) => new(parent, child);
        public static Label Label(string name) => new(name);
        public static Member Member(ICfgNode parent, ICfgNode child) => new(parent, child);
        public static Name Name(string name) => new(name);
        public static Negation Negation(ICfgNode expression) => new(expression);
        public static Numeric Const(int num) => new(num);
        public static Sequence Seq(params ICfgNode[] statements) => new(statements);
        public static SeseRegion Sese(ControlFlowGraph cfg) => new(cfg);
        public static Statement Statement(ICfgNode head, params ICfgNode[] parameters) => new(head, parameters);
        public static WhileLoop While(ICfgNode condition, ICfgNode body) => new(condition, body);
    }
}