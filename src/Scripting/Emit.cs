using System;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public static class Emit
{
    public static SingleEvent Event(IEvent e, int originalIndex) => new(e ?? throw new ArgumentNullException(nameof(e)), originalIndex);
    public static BreakStatement Break() => new();
    public static ContinueStatement Continue() => new();
    public static ControlFlowNode Cfg(ControlFlowGraph graph) => new(graph ?? throw new ArgumentNullException(nameof(graph)));
    public static DoLoop Do(ICfgNode condition, ICfgNode body) => new(condition, body);
    public static EmptyNode Empty() => new();
    public static EndlessLoop Loop(ICfgNode body) => new(body);
    public static IfThen If(ICfgNode condition, ICfgNode body) => new(condition, body);
    public static IfThenElse IfElse(ICfgNode condition, ICfgNode body, ICfgNode elseBody) => new(condition, body, elseBody);
    public static Goto Goto(string label) => new(label ?? throw new ArgumentNullException(nameof(label)));
    public static Label Label(string name) => new(name ?? throw new ArgumentNullException(nameof(name)));
    public static Name Name(string name) => new(name ?? throw new ArgumentNullException(nameof(name)));
    public static Negation Negation(ICfgNode expression) => new(expression ?? throw new ArgumentNullException(nameof(expression)));
    public static Numeric Const(int num) => new(num);
    public static Sequence Seq(params ICfgNode[] statements) => new(statements);
    public static Sequence Seq(ICfgNode head, ICfgNode tail)
    {
        var headSeq  = head as Sequence;
        var tailSeq = tail as Sequence;

        if (headSeq != null && tailSeq != null)
        {
            var args = new ICfgNode[headSeq.Statements.Length + tailSeq.Statements.Length];
            Array.Copy(headSeq.Statements, 0, args, 0, headSeq.Statements.Length);
            Array.Copy(tailSeq.Statements, 0, args, headSeq.Statements.Length, tailSeq.Statements.Length);
            return new Sequence(args);
        }

        if (headSeq != null)
        {
            var args = new ICfgNode[headSeq.Statements.Length + 1];
            Array.Copy(headSeq.Statements, 0, args, 0, headSeq.Statements.Length);
            args[^1] = tail;
            return new Sequence(args);
        }

        if (tailSeq != null)
        {
            var args = new ICfgNode[1 + tailSeq.Statements.Length];
            args[0] = head;
            Array.Copy(tailSeq.Statements, 0, args, 1, tailSeq.Statements.Length);
            return new Sequence(args);
        }

        return new Sequence(new[] { head, tail });
    }

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