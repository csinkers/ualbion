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
    public ScriptEventSet(AssetId id, StringSetId stringSetId, IList<EventNode> events)
    {
        Id = id;
        StringSetId = stringSetId;
        Events = events?.ToArray() ?? throw new ArgumentNullException(nameof(events));
    }

    public AssetId Id { get; }
    public StringSetId StringSetId { get; }
    public IList<EventNode> Events { get; }
    public IList<ushort> Chains { get; } = [0];
    public IList<ushort> ExtraEntryPoints { get; } = Array.Empty<ushort>();
    public DecompilationResult Decompiled { get; set; }
    public ushort GetChainForEvent(ushort index) => 0;
}
