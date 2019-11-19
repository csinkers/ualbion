using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class Map3D : Component, IMap
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Map3D, WorldCoordinateSelectEvent>((x, e) => x.Select(e))
            // H<Map3D, UnloadMapEvent>((x, e) => x.Unload()),
        );

        Skybox _skybox;
        MapRenderable3D _renderable;
        LabyrinthData _labyrinthData;
        MapData3D _mapData;
        float _backgroundRed;
        float _backgroundGreen;
        float _backgroundBlue;

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
        }

        public Map3D(MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
        }

        public override string ToString() => $"Map3D:{MapId} {LogicalSize.X}x{LogicalSize.Y} TileSize: {TileSize}";
        public MapDataId MapId { get; }
        public Vector2 LogicalSize { get; private set; }
        public Vector3 TileSize { get; private set; }

        void LoadMap()
        {
            var assets = Resolve<IAssetManager>();
            _mapData = assets.LoadMap3D(MapId);
            _labyrinthData = assets.LoadLabyrinthData(_mapData.LabDataId);

            if (_labyrinthData == null)
            {
                TileSize = Vector3.One * 512;
                return;
            }

            TileSize = new Vector3(_labyrinthData.EffectiveWallWidth, _labyrinthData.WallHeight,
                _labyrinthData.EffectiveWallWidth);

            _renderable = new MapRenderable3D(MapId, assets, _mapData, _labyrinthData, TileSize);
            Exchange.Attach(_renderable);
            Children.Add(_renderable);

            if (_labyrinthData.BackgroundId.HasValue)
            {
                _skybox = new Skybox(assets, _labyrinthData.BackgroundId.Value, _mapData.PaletteId);
                Exchange.Attach(_skybox);
                Children.Add(_skybox);
            }

            var palette = assets.LoadPalette(_mapData.PaletteId);
            uint backgroundColour = palette.GetPaletteAtTime(0)[_labyrinthData.BackgroundColour];
            _backgroundRed = (backgroundColour & 0xff) / 255.0f;
            _backgroundGreen = (backgroundColour & 0xff00 >> 8) / 255.0f;
            _backgroundBlue = (backgroundColour & 0xff0000 >> 16) / 255.0f;

            //if(_labyrinthData.CameraHeight != 0)
            //    Debugger.Break();

            //if(_labyrinthData.Unk12 != 0) // 7=1|2|4 (Jirinaar), 54=32|16|4|2, 156=128|16|8|2 (Tall town)
            //    Debugger.Break();

            var maxObjectHeightRaw = _labyrinthData.ObjectGroups.Max(x => x.SubObjects.Max(y => (int?)y.Y));
            float objectYScaling = TileSize.Y / _labyrinthData.WallHeight;
            if (maxObjectHeightRaw > _labyrinthData.WallHeight * 1.5f)
                objectYScaling /= 2; // TODO: Figure out the proper way to handle this.

            Raise(new LogEvent(LogEvent.Level.Info, $"WallHeight: {_labyrinthData.WallHeight} MaxObj: {maxObjectHeightRaw} EffWallWidth: {_labyrinthData.EffectiveWallWidth}"));

            foreach (var npc in _mapData.Npcs)
            {
                var objectData = _labyrinthData.ObjectGroups[npc.ObjectNumber - 1];
                foreach (var subObject in objectData.SubObjects)
                {
                    var sprite = BuildSprite(npc.Waypoints[0].X, npc.Waypoints[0].Y, subObject, objectYScaling);
                    if (sprite == null)
                        continue;

                    Exchange.Attach(sprite);
                    Children.Add(sprite);
                }
            }

            for (int y = 0; y < _mapData.Height; y++)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    var contents = _mapData.Contents[y * _mapData.Width + x];
                    if (contents == 0 || contents >= _labyrinthData.ObjectGroups.Count)
                        continue;

                    var objectInfo = _labyrinthData.ObjectGroups[contents - 1];
                    foreach (var subObject in objectInfo.SubObjects)
                    {
                        var sprite = BuildSprite(x, y, subObject, objectYScaling);
                        if (sprite == null)
                            continue;

                        Exchange.Attach(sprite);
                        Children.Add(sprite);
                    }
                }
            }
        }

        protected override void Subscribed()
        {
            if (_mapData == null)
                LoadMap();
            Raise(new SetClearColourEvent(_backgroundRed, _backgroundGreen, _backgroundBlue));
            Raise(new SetTileSizeEvent(TileSize, _labyrinthData.CameraHeight != 0 ? _labyrinthData.CameraHeight * 8 : TileSize.Y/2));
        }

        MapObjectSprite BuildSprite(int tileX, int tileY, LabyrinthData.SubObject subObject, float objectYScaling)
        {
            var definition = _labyrinthData.Objects[subObject.ObjectInfoNumber];
            if (definition.TextureNumber == null)
                return null;

            bool onFloor = (definition.Properties & LabyrinthData.Object.ObjectFlags.FloorObject) != 0;

            // We should probably be offsetting the main tilemap by half a tile to centre the objects
            // rather than fiddling with the object positions... will need to reevaluate when working on
            // collision detection, path-finding etc.
            var objectBias = new Vector3(-1.0f, 0, -1.0f); // / 2;
            var tilePosition = (new Vector3(tileX, 0, tileY) + objectBias) * TileSize;
            var offset = new Vector3(
                subObject.X,
                subObject.Y * objectYScaling,
                subObject.Z);

            var smidgeon = onFloor 
                ? new Vector3(0,offset.Y < float.Epsilon ? 0.5f : -0.5f, 0) 
                : Vector3.Zero;

            var position = tilePosition + offset + smidgeon;

            return new MapObjectSprite(
                definition.TextureNumber.Value,
                position,
                new Vector2(definition.MapWidth, definition.MapHeight),
                (definition.Properties & LabyrinthData.Object.ObjectFlags.FloorObject) != 0
            );
        }
    }
}
