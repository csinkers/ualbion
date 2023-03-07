using System.Linq;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting.Rules;

public static class LoopConverter
{
    public static (ControlFlowGraph result, string description) Apply(ControlFlowGraph graph)
    {
        var visitor = new LoopConversionVisitor();
        return (graph.AcceptBuilder(visitor), "Convert loops");
    }

    class LoopConversionVisitor : BaseAstBuilderVisitor
    {
        protected override ICfgNode Build(EndlessLoop loop)
        {
            // Patterns:
            // Loop[If[c, break]]            => while (!c) { }
            // Loop[Seq[If[c, break], rest]] => while (!c) { rest }
            // Loop[Seq[rest, If[c, break]]] => do { rest } while (!c)

            static ICfgNode SeqRemoveHead(Sequence seq) =>
                seq.Statements.Length == 2
                    ? seq.Statements[1]
                    : UAEmit.Seq(seq.Statements.Skip(1).ToArray());

            static ICfgNode SeqRemoveTail(Sequence seq) =>
                seq.Statements.Length == 2
                    ? seq.Statements[0]
                    : UAEmit.Seq(seq.Statements.Take(seq.Statements.Length - 1).ToArray());

            if (loop.Body is IfThen(Negation n1, BreakStatement))
            {
                n1.Expression.Accept(this);
                var condition = Result ?? n1.Expression;
                return UAEmit.While(condition, null);
            }

            if (loop.Body is IfThen(var condition1, BreakStatement))
            {
                condition1.Accept(this);
                var condition = Result ?? condition1;
                return UAEmit.While(UAEmit.Negation(condition), null);
            }

            if (loop.Body is not Sequence seq)
            {
                loop.Body.Accept(this);
                return Result != null ? UAEmit.Loop(Result) : null;
            }

            if (seq.Statements[0] is IfThen(Negation n2, BreakStatement))
            {
                n2.Expression.Accept(this);
                var condition = Result ?? n2.Expression;

                var newSeq = SeqRemoveHead(seq);
                newSeq.Accept(this);
                newSeq = Result ?? newSeq;
                return UAEmit.While(condition, newSeq);
            }

            if (seq.Statements[0] is IfThen(var condition2, BreakStatement))
            {
                condition2.Accept(this);
                var condition = Result ?? condition2;

                var newSeq = SeqRemoveHead(seq);
                newSeq.Accept(this);
                newSeq = Result ?? newSeq;
                return UAEmit.While(UAEmit.Negation(condition), newSeq);
            }

            if (seq.Statements[^1] is IfThen(Negation n3, BreakStatement))
            {
                n3.Expression.Accept(this);
                var condition = Result ?? n3.Expression;

                var newSeq = SeqRemoveTail(seq);
                newSeq.Accept(this);
                newSeq = Result ?? newSeq;
                return UAEmit.Do(condition, newSeq);
            }

            if (seq.Statements[^1] is IfThen(var condition3, BreakStatement))
            {
                condition3.Accept(this);
                var condition = Result ?? condition3;

                var newSeq = SeqRemoveTail(seq);
                newSeq.Accept(this);
                newSeq = Result ?? newSeq;
                return UAEmit.Do(UAEmit.Negation(condition), newSeq);
            }

            loop.Body.Accept(this);
            return Result != null ? UAEmit.Loop(Result) : null;
        }
    }
}