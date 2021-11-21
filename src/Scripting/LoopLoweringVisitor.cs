using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public class LoopLoweringVisitor : BaseBuilderAstVisitor
    {
        readonly Stack<string> _headStack = new();
        readonly Stack<string> _tailStack = new();
        protected override ICfgNode Build(DoLoop doLoop)
        {
            var headLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var tailLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            _headStack.Push(headLabel);
            _tailStack.Push(tailLabel);

            doLoop.Body?.Accept(this);
            var body = Result;
            doLoop.Condition.Accept(this);
            var condition = Result;

            _tailStack.Pop();
            _headStack.Pop();

            return Emit.Cfg(new ControlFlowGraph(new[]
                {
                    Emit.Empty(), // 0
                    Emit.Label(headLabel), // 1
                    body ?? doLoop.Body, // 2
                    condition ?? doLoop.Condition, // 3
                    Emit.Label(tailLabel), // 4
                    Emit.Empty() // 5
                },
                new[]
                {
                    (0,1,true), (1,2,true), (2,3,true),
                    (3,1,true),
                    (3,4,false),
                    (4,5,true),
                }));
        }

        protected override ICfgNode Build(WhileLoop whileLoop)
        {
            var headLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            var tailLabel = ScriptConstants.BuildDummyLabel(Guid.NewGuid());
            _headStack.Push(headLabel);
            _tailStack.Push(tailLabel);

            whileLoop.Condition.Accept(this);
            var condition = Result;
            whileLoop.Body?.Accept(this);
            var body = Result;

            _tailStack.Pop();
            _headStack.Pop();

            return Emit.Cfg(new ControlFlowGraph(new[]
                {
                    Emit.Empty(), // 0
                    Emit.Label(headLabel), // 1
                    condition ?? whileLoop.Condition, // 2
                    body ?? whileLoop.Body, // 3
                    Emit.Label(tailLabel), // 4
                    Emit.Empty() // 5
                },
                new[]
                {
                    (0,1,true), (1,2,true), 
                    (2,3,true), (2,4,false),
                    (3,1,true),
                    (4,5,true)
                }));
        }

        protected override ICfgNode Build(BreakStatement breakStatement)
        {
            if (_headStack.Count == 0)
                throw new InvalidOperationException("Break statement detected outside of a loop");
            return Emit.Goto(_tailStack.Peek());
        }

        protected override ICfgNode Build(ContinueStatement continueStatement)
        {
            if (_headStack.Count == 0)
                throw new InvalidOperationException("Continue statement detected outside of a loop");
            return Emit.Goto(_headStack.Peek());
        }
    }
}