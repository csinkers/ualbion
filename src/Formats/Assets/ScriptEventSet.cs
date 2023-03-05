using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Scripting;

namespace UAlbion.Formats.Assets;

public class ScriptEventSet : IEventSet
{
    public ScriptEventSet(AssetId id, TextId textId, IList<EventNode> events)
    {
        Id = id;
        TextId = textId;
        Events = events?.ToArray() ?? throw new ArgumentNullException(nameof(events));
    }

    public AssetId Id { get; }
    public TextId TextId { get; }
    public IList<EventNode> Events { get; }
    public IList<ushort> Chains { get; } = Array.Empty<ushort>();
    public IList<ushort> ExtraEntryPoints { get; } = Array.Empty<ushort>();
    public DecompilationResult Decompiled { get; set; }
    public ushort GetChainForEvent(ushort index) => 0;
}
