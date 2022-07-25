using System;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class EventTransformVisitor : BaseAstBuilderVisitor
{
    readonly Func<IEvent, IEvent> _transform;
    public EventTransformVisitor(Func<IEvent, IEvent> transform) => _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    protected override ICfgNode Build(SingleEvent e)
    {
        var transformed = _transform(e.Event);
        return transformed == null || transformed == e.Event ? null : Emit.Event(transformed, e.OriginalIndex);
    }
}