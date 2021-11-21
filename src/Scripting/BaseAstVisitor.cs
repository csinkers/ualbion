using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public abstract class BaseAstVisitor : IAstVisitor
    {
        public virtual void Visit(SingleEvent e) { }
        public virtual void Visit(BreakStatement breakStatement) { }
        public virtual void Visit(ContinueStatement continueStatement) { }
        public virtual void Visit(ControlFlowNode cfgNode) { }
        public virtual void Visit(EmptyNode empty) { }
        public virtual void Visit(Goto jump) { }
        public virtual void Visit(Label label) { } 
        public virtual void Visit(Name name) { }
        public virtual void Visit(Numeric numeric) { }

        public virtual void Visit(Negation negation) => negation.Expression.Accept(this);
        public virtual void Visit(IfThen ifThen)
        {
            ifThen.Condition.Accept(this);
            ifThen.Body?.Accept(this);
        }

        public virtual void Visit(IfThenElse ifElse)
        {
            ifElse.Condition.Accept(this);
            ifElse.TrueBody?.Accept(this);
            ifElse.FalseBody?.Accept(this);
        }

        public virtual void Visit(Statement statement)
        {
            statement.Head.Accept(this);
            foreach (var part in statement.Parameters) part.Accept(this);
        }

        public virtual void Visit(WhileLoop whileLoop)
        {
            whileLoop.Condition.Accept(this);
            whileLoop.Body?.Accept(this);
        }

        public virtual void Visit(Sequence sequence)
        {
            foreach (var node in sequence.Statements)
                node.Accept(this);
        }

        public virtual void Visit(DoLoop doLoop)
        {
            doLoop.Body?.Accept(this);
            doLoop.Condition.Accept(this);
        }

        public virtual void Visit(BinaryOp binaryOp)
        {
            binaryOp.Left.Accept(this);
            binaryOp.Right.Accept(this);
        }
    }
}