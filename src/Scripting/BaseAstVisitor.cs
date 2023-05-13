using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public abstract class BaseAstVisitor : IAstVisitor
{
    public virtual void Visit(SingleEvent e) { }
    public virtual void Visit(BreakStatement breakStatement) { }
    public virtual void Visit(ContinueStatement continueStatement) { }
    public virtual void Visit(ControlFlowNode cfgNode) { }
    public virtual void Visit(EmptyNode empty) { }
    public virtual void Visit(GotoStatement jump) { }
    public virtual void Visit(Label label) { } 
    public virtual void Visit(Name name) { }
    public virtual void Visit(Numeric numeric) { }

    public virtual void Visit(Negation negation) => negation?.Expression.Accept(this);
    public virtual void Visit(IfThen ifThen)
    {
        if (ifThen == null) return;
        ifThen.Condition.Accept(this);
        ifThen.Body?.Accept(this);
    }

    public virtual void Visit(IfThenElse ifElse)
    {
        if (ifElse == null) return;
        ifElse.Condition.Accept(this);
        ifElse.TrueBody?.Accept(this);
        ifElse.FalseBody?.Accept(this);
    }

    public virtual void Visit(Statement statement)
    {
        if (statement == null) return;
        statement.Head.Accept(this);
        foreach (var part in statement.Parameters) part.Accept(this);
    }

    public virtual void Visit(Sequence sequence)
    {
        if (sequence == null) return;
        foreach (var node in sequence.Statements)
            node.Accept(this);
    }

    public virtual void Visit(DoLoop doLoop)
    {
        if (doLoop == null) return;
        doLoop.Body?.Accept(this);
        doLoop.Condition.Accept(this);
    }

    public virtual void Visit(EndlessLoop endlessLoop)
    {
        if (endlessLoop == null) return;
        endlessLoop.Body?.Accept(this);
    }

    public virtual void Visit(WhileLoop whileLoop)
    {
        if (whileLoop == null) return;
        whileLoop.Condition.Accept(this);
        whileLoop.Body?.Accept(this);
    }

    public virtual void Visit(BinaryOp binaryOp)
    {
        if (binaryOp == null) return;
        binaryOp.Left.Accept(this);
        binaryOp.Right.Accept(this);
    }
}