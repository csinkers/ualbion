<Query Kind="Program">
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.Api.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.Api.dll</Reference>
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.Base.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.Base.dll</Reference>
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.Config.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.Config.dll</Reference>
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.Core.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.Core.dll</Reference>
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.dll</Reference>
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.Formats.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.Formats.dll</Reference>
  <Reference Relative="..\..\build\UAlbion\bin\Debug\net6.0\UAlbion.Game.dll">C:\Depot\bb\ualbion\build\UAlbion\bin\Debug\net6.0\UAlbion.Game.dll</Reference>
  <Namespace>UAlbion</Namespace>
  <Namespace>UAlbion.Api</Namespace>
  <Namespace>UAlbion.Base</Namespace>
  <Namespace>UAlbion.Config</Namespace>
  <Namespace>UAlbion.Core</Namespace>
  <Namespace>UAlbion.Formats</Namespace>
  <Namespace>UAlbion.Formats.Assets.Maps</Namespace>
  <Namespace>UAlbion.Formats.Config</Namespace>
  <Namespace>UAlbion.Game</Namespace>
  <Namespace>UAlbion.Game.Settings</Namespace>
  <Namespace>UAlbion.Formats.Assets</Namespace>
</Query>

const string baseDir = @"C:\Depot\bb\ualbion";

void Main()
{
	var disk = new FileSystem();
	disk.CurrentDirectory = baseDir;
	var exchange = AssetSystem.SetupSimple(disk, AssetMapping.Global, "Base");
	var assets = exchange.Resolve<IAssetManager>();
	var layerMap = new Dictionary<TileLayer, int>();
	var typeMap = new Dictionary<TileType, int>();
	var flagMap = new Dictionary<TileFlags, int>();
	var unk7Map = new Dictionary<byte, int>();
	var collisionMap = new Dictionary<Passability, int>();
	var depthMap = new Dictionary<int, int>();

	void Evaluate(bool isOverlay, int[] layer, TilesetData tileset)
	{
		for (int i = 0; i < layer.Length; i++)
		{
			var tile = tileset.Tiles[layer[i]];
			Set(isOverlay, layerMap, tile.Layer);
			Set(isOverlay, typeMap, tile.Type);
			// foreach (var flag in SeparateFlags(tile.Flags))
			// 	Set(isOverlay, flagMap, flag);
			Set(isOverlay, unk7Map, tile.Unk7);
			Set(isOverlay, collisionMap, tile.Collision);
			Set(isOverlay, depthMap, tile.Depth);
		}
	}
	
	var ruffians = new List<MapId>();
	foreach (var mapId in Enum.GetValues(typeof(UAlbion.Base.Map)).OfType<UAlbion.Base.Map>())
	{
		var map = assets.LoadMap(mapId);
		if (map is not MapData2D map2d)
			continue;

		var tileset = assets.LoadTileData(map2d.TilesetId);
		Evaluate(false, map2d.Underlay, tileset);
		Evaluate(true, map2d.Overlay, tileset);

		var underlayIds = map2d.Underlay.ToHashSet();
		var overlayIds = map2d.Overlay.ToHashSet();
		var shared = underlayIds.Intersect(overlayIds).Where(x => x != 0).OrderBy(x => x).ToList();
		if (shared.Count > 0)
		{
			new
			{
				Map = mapId,
				Tileset = map2d.TilesetId.ToString(),
				SharedTiles = shared.Select(x => (x, tileset.Tiles[x]))
			}.Dump();
		}
	}
	new
	{
		Layers = layerMap.OrderBy(x => x.Key),
		Types = typeMap.OrderBy(x => x.Key),
		Flags = flagMap.OrderBy(x => x.Key),
		Unk7 = unk7Map.OrderBy(x => x.Key),
		Collision = collisionMap.OrderBy(x => x.Key),
		Depth = depthMap.OrderBy(x => x.Key)
	}.Dump();
}

void Set<T>(bool isOverlay, Dictionary<T, int> dict, T key)
{
	dict.TryGetValue(key, out var value);
	dict[key] = value | (isOverlay ? 2 : 1);
}

IEnumerable<TileFlags> SeparateFlags(TileFlags combined)
{
	for(int i = 1; i < (1 << 15); i <<= 1)
		if (((int)combined & i) != 0)
			yield return (TileFlags)i;
}
