﻿using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public interface IEventSet
{
    AssetId Id { get; }
    TextId TextId { get; }
    List<ushort> Chains { get; }
    List<EventNode> Events { get; }
    ushort GetChainForEvent(ushort index);
}