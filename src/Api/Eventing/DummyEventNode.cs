using System;

namespace UAlbion.Api.Eventing;

public class DummyEventNode : IEventNode // These should only exist temporarily during deserialisation
{
    public DummyEventNode(ushort id) => Id = id;
    public ushort Id { get; }
    public IEvent Event => throw new InvalidOperationException("All DummyEventNodes should be removed during the unswizzling process.");
    public IEventNode Next => throw new InvalidOperationException("All DummyEventNodes should be removed during the unswizzling process.");
    public override string ToString()
    {
        var builder = new UnformattedScriptBuilder(false);
        Format(builder, 0);
        return builder.Build();
    }

    public void Format(IScriptBuilder builder, int idOffset)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Append("DummyNode ");
        builder.Append(Id - idOffset);
    }
}
