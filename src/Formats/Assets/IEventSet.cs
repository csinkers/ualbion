﻿using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Scripting;

namespace UAlbion.Formats.Assets;

public interface IEventSet
{
    AssetId Id { get; }
    StringSetId StringSetId { get; }
    IList<EventNode> Events { get; }
    IList<ushort> Chains { get; }
    IList<ushort> ExtraEntryPoints { get; }
    ushort GetChainForEvent(ushort index);
    DecompilationResult Decompiled { get; set; }
}