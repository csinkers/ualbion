﻿using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Entities;

public interface IMap : IComponent
{
    MapId MapId { get; }
    MapType MapType { get; }
    Vector3 TileSize { get; }
    float BaseCameraHeight { get; }
    IMapData MapData { get; }
}