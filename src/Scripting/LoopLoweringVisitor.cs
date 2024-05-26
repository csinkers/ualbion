using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class LoopLoweringVisitor : BaseAstBuilderVisitor
{
    readonly Stack<string> _headStack = new();
    readonly Stack<string> _tailStack = new();
    protected override ICfgNode Build(DoLoop doLoop)
    {
        ArgumentNullException.ThrowIfNull(doLoop);

        var headLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        var tailLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        _headStack.Push(headLabel);
        _tailStack.Push(tailLabel);

        doLoop.Body?.Accept(this);
        var body = Result ?? doLoop.Body;
        doLoop.Condition.Accept(this);
        var condition = Result ?? doLoop.Condition;

        _tailStack.Pop();
        _headStack.Pop();

        var negated = false;
        if (condition is Negation negation)
        {
            condition = negation.Expression;
            negated = true;
        }

        return UAEmit.Cfg(new ControlFlowGraph(new[]
            {
                UAEmit.Empty(), // 0
                UAEmit.Label(headLabel), // 1
                body, // 2
                condition, // 3
                UAEmit.Label(tailLabel), // 4
                UAEmit.Empty() // 5
            },
            new[]
            {
                (0,1,CfgEdge.True), (1,2,CfgEdge.True), (2,3,CfgEdge.True),
                (3,1,negated ? CfgEdge.False : CfgEdge.True), (3,4,negated ? CfgEdge.True : CfgEdge.False),
                (4,5,CfgEdge.True),
            }));
    }

    protected override ICfgNode Build(WhileLoop whileLoop)
    {
        ArgumentNullException.ThrowIfNull(whileLoop);

        var headLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        var tailLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        _headStack.Push(headLabel);
        _tailStack.Push(tailLabel);

        whileLoop.Condition.Accept(this);
        var condition = Result ?? whileLoop.Condition;
        whileLoop.Body?.Accept(this);
        var body = Result ?? whileLoop.Body;

        _tailStack.Pop();
        _headStack.Pop();

        var negated = false;
        if (condition is Negation negation)
        {
            condition = negation.Expression;
            negated = true;
        }

        return UAEmit.Cfg(new ControlFlowGraph(new[]
            {
                UAEmit.Empty(), // 0
                UAEmit.Label(headLabel), // 1
                condition, // 2
                body, // 3
                UAEmit.Label(tailLabel), // 4
                UAEmit.Empty() // 5
            },
            new[]
            {
                (0,1,CfgEdge.True), (1,2,CfgEdge.True),
                (2,3,negated ? CfgEdge.False : CfgEdge.True), (2,4,negated ? CfgEdge.True : CfgEdge.False),
                (3,1,CfgEdge.True),
                (4,5,CfgEdge.True)
            }));
    }

    protected override ICfgNode Build(EndlessLoop endlessLoop)
    {
        ArgumentNullException.ThrowIfNull(endlessLoop);

        var headLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        var tailLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
        _headStack.Push(headLabel);
        _tailStack.Push(tailLabel);

        endlessLoop.Body?.Accept(this);
        var body = Result ?? endlessLoop.Body;

        _tailStack.Pop();
        _headStack.Pop();

        return UAEmit.Cfg(new ControlFlowGraph(new[]
            {
                UAEmit.Empty(), // 0
                UAEmit.Label(headLabel), // 1
                body, // 2
                UAEmit.Label(tailLabel), // 3
                UAEmit.Empty() // 4
            },
            new[]
            {
                (0,1,CfgEdge.True),
                (1,2,CfgEdge.True), (1,3,CfgEdge.LoopSuccessor),
                (2,1,CfgEdge.True),
                (3,4,CfgEdge.True)
            }));
    }

    protected override ICfgNode Build(BreakStatement breakStatement)
    {
        if (_headStack.Count == 0)
            throw new InvalidOperationException("Break statement detected outside of a loop");
        return UAEmit.Goto(_tailStack.Peek());
    }

    protected override ICfgNode Build(ContinueStatement continueStatement)
    {
        if (_headStack.Count == 0)
            throw new InvalidOperationException("Continue statement detected outside of a loop");
        return UAEmit.Goto(_headStack.Peek());
    }
}