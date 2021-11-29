namespace UAlbion.Scripting.Ast
{
    public interface IAstVisitor
    {
        void Visit(BinaryOp binaryOp);
        void Visit(BreakStatement breakStatement);
        void Visit(ContinueStatement continueStatement);
        void Visit(ControlFlowNode cfgNode);
        void Visit(DoLoop doLoop);
        void Visit(EmptyNode empty);
        void Visit(EndlessLoop loop);
        void Visit(IfThen ifThen);
        void Visit(IfThenElse ifElse);
        void Visit(Goto jump);
        void Visit(Label label);
        void Visit(Name name);
        void Visit(Negation negation);
        void Visit(Numeric numeric);
        void Visit(Sequence sequence);
        void Visit(SingleEvent e);
        void Visit(Statement statement);
        void Visit(WhileLoop whileLoop);
    }
}