using System.Collections.Generic;
using System.Linq;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public abstract class BaseAstBuilderVisitor : IAstBuilderVisitor
{
    public ICfgNode Result { get; private set; }

    public void Visit(SingleEvent e) => Result = Build(e);
    public void Visit(BinaryOp binaryOp) => Result = Build(binaryOp);
    public void Visit(BreakStatement breakStatement) => Result = Build(breakStatement);
    public void Visit(ContinueStatement continueStatement) => Result = Build(continueStatement);
    public void Visit(ControlFlowNode cfgNode) => Result = Build(cfgNode);
    public void Visit(DoLoop doLoop) => Result = Build(doLoop);
    public void Visit(EmptyNode empty) => Result = Build(empty);
    public void Visit(EndlessLoop loop) => Result = Build(loop);
    public void Visit(Goto jump) => Result = Build(jump);
    public void Visit(IfThen ifThen) => Result = Build(ifThen);
    public void Visit(IfThenElse ifElse) => Result = Build(ifElse);
    public void Visit(Label label) => Result = Build(label);
    public void Visit(Name name) => Result = Build(name);
    public void Visit(Negation negation) => Result = Build(negation);
    public void Visit(Numeric numeric) => Result = Build(numeric);
    public void Visit(Sequence sequence) => Result = Build(sequence);
    public void Visit(Statement statement) => Result = Build(statement);
    public void Visit(WhileLoop whileLoop) => Result = Build(whileLoop);

    protected virtual ICfgNode Build(SingleEvent e) => null;
    protected virtual ICfgNode Build(BreakStatement breakStatement) => null;
    protected virtual ICfgNode Build(ContinueStatement continueStatement) => null;
    protected virtual ICfgNode Build(ControlFlowNode cfgNode) => null;
    protected virtual ICfgNode Build(EmptyNode empty) => null;
    protected virtual ICfgNode Build(Goto jump) => null;
    protected virtual ICfgNode Build(Label label) => null;
    protected virtual ICfgNode Build(Name name) => null;
    protected virtual ICfgNode Build(Numeric numeric) => null;

    protected virtual ICfgNode Build(Negation negation)
    {
        negation.Expression.Accept(this);
        return Result == null ? null : Emit.Negation(Result);
    }

    protected virtual ICfgNode Build(IfThen ifThen)
    {
        ifThen.Condition.Accept(this);
        var condition = Result;
        ifThen.Body?.Accept(this);
        var body = Result;

        if (condition == null && body == null)
            return null;

        return Emit.If(condition ?? ifThen.Condition, body ?? ifThen.Body);
    }

    protected virtual ICfgNode Build(IfThenElse ifElse)
    {
        ifElse.Condition.Accept(this);
        var condition = Result;
        ifElse.TrueBody?.Accept(this);
        var trueBody = Result;

        ifElse.FalseBody?.Accept(this);
        var falseBody = Result;

        if (condition == null && trueBody == null && falseBody == null)
            return null;

        ifElse.FalseBody?.Accept(this);
        return Emit.IfElse(
            condition ?? ifElse.Condition,
            trueBody ?? ifElse.TrueBody,
            falseBody ?? ifElse.FalseBody);
    }

    protected virtual ICfgNode Build(Statement statement)
    {
        statement.Head.Accept(this);
        var head = Result;
        var parts = new ICfgNode[statement.Parameters.Length];
        for (var index = 0; index < statement.Parameters.Length; index++)
        {
            statement.Parameters[index].Accept(this);
            parts[index] = Result;
        }

        if (head == null && parts.All(x => x == null))
            return null;

        for (var index = 0; index < statement.Parameters.Length; index++)
            parts[index] ??= statement.Parameters[index];

        return Emit.Statement(head, parts);
    }

    protected virtual ICfgNode Build(Sequence sequence)
    {
        var result = new List<ICfgNode>();
        bool trivial = true;
        foreach (var node in sequence.Statements)
        {
            node.Accept(this);
            if (Result != null)
                trivial = false;

            if (Result is Sequence seq)
            {
                foreach (var statement in seq.Statements)
                    result.Add(statement);
            }
            else result.Add(Result ?? node);
        }

        if (trivial)
            return null;

        return Emit.Seq(result.ToArray());
    }

    protected virtual ICfgNode Build(DoLoop doLoop)
    {
        doLoop.Body?.Accept(this);
        var body = Result;
        doLoop.Condition.Accept(this);
        var condition = Result;

        if (condition == null && body == null)
            return null;

        return Emit.Do(
            condition ?? doLoop.Condition,
            body ?? doLoop.Body);
    }

    protected virtual ICfgNode Build(EndlessLoop loop)
    {
        loop.Body?.Accept(this);
        var body = Result;

        if (body == null)
            return null;

        return Emit.Loop(body);
    }

    protected virtual ICfgNode Build(WhileLoop whileLoop)
    {
        whileLoop.Condition.Accept(this);
        var condition = Result;
        whileLoop.Body?.Accept(this);
        var body = Result;

        if (condition == null && body == null)
            return null;

        return Emit.While(
            condition ?? whileLoop.Condition,
            body ?? whileLoop.Body);
    }

    protected virtual ICfgNode Build(BinaryOp binaryOp)
    {
        binaryOp.Left.Accept(this);
        var left = Result;
        binaryOp.Right.Accept(this);
        var right = Result;

        if (left == null && right == null)
            return null;

        return Emit.Op(
            binaryOp.Operation,
            left ?? binaryOp.Left,
            right ?? binaryOp.Right);
    }
}