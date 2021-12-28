using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class CountEventsVisitor : BaseAstVisitor
{
    public int Count { get; private set; }
    public void Reset() => Count = 0;
    public override void Visit(SingleEvent e) => Count++;
}