using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Formats.Assets;

public interface IEventSet
{
    AssetId Id { get; }
    TextId TextId { get; }
    List<ushort> Chains { get; }
    List<EventNode> Events { get; }
    ushort GetChainForEvent(ushort index);
}