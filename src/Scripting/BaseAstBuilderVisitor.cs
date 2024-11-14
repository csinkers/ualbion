﻿using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public abstract class BaseAstBuilderVisitor : IAstBuilderVisitor
{
    public ICfgNode Result { get; private set; }

    public ControlFlowGraph Apply(ControlFlowGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        foreach (var index in graph.GetDfsOrder())
        {
            var node = graph.Nodes[index];
            node.Accept(this);
            if (Result != null)
                graph = graph.ReplaceNode(index, Result);
        }

        return graph;
    }

    public void Visit(SingleEvent e) => Result = Build(e);
    public void Visit(BinaryOp binaryOp) => Result = Build(binaryOp);
    public void Visit(BreakStatement breakStatement) => Result = Build(breakStatement);
    public void Visit(ContinueStatement continueStatement) => Result = Build(continueStatement);
    public void Visit(ControlFlowNode cfgNode) => Result = Build(cfgNode);
    public void Visit(DoLoop doLoop) => Result = Build(doLoop);
    public void Visit(EmptyNode empty) => Result = Build(empty);
    public void Visit(EndlessLoop endlessLoop) => Result = Build(endlessLoop);
    public void Visit(GotoStatement jump) => Result = Build(jump);
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
    protected virtual ICfgNode Build(GotoStatement jump) => null;
    protected virtual ICfgNode Build(Label label) => null;
    protected virtual ICfgNode Build(Name name) => null;
    protected virtual ICfgNode Build(Numeric numeric) => null;

    protected virtual ICfgNode Build(Negation negation)
    {
        ArgumentNullException.ThrowIfNull(negation);
        negation.Expression.Accept(this);
        return Result == null ? null : Emit.Negation(Result);
    }

    protected virtual ICfgNode Build(IfThen ifThen)
    {
        ArgumentNullException.ThrowIfNull(ifThen);

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
        ArgumentNullException.ThrowIfNull(ifElse);

        ifElse.Condition.Accept(this);
        var condition = Result;
        ifElse.TrueBody?.Accept(this);
        var trueBody = Result;
        ifElse.FalseBody?.Accept(this);
        var falseBody = Result;

        if (condition == null && trueBody == null && falseBody == null)
            return null;

        return Emit.IfElse(
            condition ?? ifElse.Condition,
            trueBody ?? ifElse.TrueBody,
            falseBody ?? ifElse.FalseBody);
    }

    protected virtual ICfgNode Build(Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);

        statement.Head.Accept(this);
        var head = Result;
        ICfgNode[] parts = null;
        for (var index = 0; index < statement.Parameters.Length; index++)
        {
            statement.Parameters[index].Accept(this);
            if (parts == null)
            {
                if (Result == null)
                    continue;

                parts = new ICfgNode[statement.Parameters.Length];
                for (int i = 0; i < index; i++)
                    parts[i] = statement.Parameters[i];
            }

            parts[index] = Result ?? statement.Parameters[index];
        }

        if (head == null && parts== null)
            return null;

        head ??= statement.Head;
        parts ??= statement.Parameters;

        return Emit.Statement(head, parts);
    }

    protected virtual ICfgNode Build(Sequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        List<ICfgNode> result = null;
        for (var index = 0; index < sequence.Statements.Length; index++)
        {
            var node = sequence.Statements[index];
            node.Accept(this);
            if (result == null)
            {
                if (Result == null)
                    continue;

                result = [];
                for (int i = 0; i < index; i++)
                    result.Add(sequence.Statements[i]);
            }

            if (Result is Sequence seq)
            {
                foreach (var statement in seq.Statements)
                    result.Add(statement);
            }
            else result.Add(Result ?? node);
        }

        return result == null ? null : Emit.Seq(result.ToArray());
    }

    protected virtual ICfgNode Build(DoLoop doLoop)
    {
        ArgumentNullException.ThrowIfNull(doLoop);

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

    protected virtual ICfgNode Build(EndlessLoop endlessLoop)
    {
        ArgumentNullException.ThrowIfNull(endlessLoop);

        endlessLoop.Body?.Accept(this);
        var body = Result;

        if (body == null)
            return null;

        return Emit.Loop(body);
    }

    protected virtual ICfgNode Build(WhileLoop whileLoop)
    {
        ArgumentNullException.ThrowIfNull(whileLoop);

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
        ArgumentNullException.ThrowIfNull(binaryOp);

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