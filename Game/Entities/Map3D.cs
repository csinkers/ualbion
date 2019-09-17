using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class Map3D : Component, IMap
    {
        readonly Skybox _skybox;
        readonly MapRenderable3D _renderable;
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Map3D, SubscribedEvent>((x, e) => x.Subscribed()),
            new Handler<Map3D, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
            // new Handler<Map3D, UnloadMapEvent>((x, e) => x.Unload()),
        };

        readonly LabyrinthData _labyrinthData;
        readonly MapData3D _mapData;
        readonly float _backgroundRed;
        readonly float _backgroundGreen;
        readonly float _backgroundBlue;

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
        }

        public Map3D(Assets assets, MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
            _mapData = assets.LoadMap3D(mapId);

            _labyrinthData = assets.LoadLabyrinthData(_mapData.LabDataId);
            if (_labyrinthData != null)
            {
                //TileSize = new Vector3(64.0f, 64.0f * (_labyrinthData.WallHeight) / 512.0f, 64.0f);
                TileSize = new Vector3(_labyrinthData.EffectiveWallWidth, _labyrinthData.WallHeight,
                    _labyrinthData.EffectiveWallWidth);
                _renderable = new MapRenderable3D(assets, _mapData, _labyrinthData, TileSize);
                if (_labyrinthData.BackgroundId.HasValue)
                    _skybox = new Skybox(assets, _labyrinthData.BackgroundId.Value, _mapData.PaletteId);

                var palette = assets.LoadPalette(_mapData.PaletteId);
                uint backgroundColour = palette.GetPaletteAtTime(0)[_labyrinthData.BackgroundColour];
                _backgroundRed   = (backgroundColour & 0xff) / 255.0f;
                _backgroundGreen = (backgroundColour & 0xff00 >> 8) / 255.0f;
                _backgroundBlue  = (backgroundColour & 0xff0000 >> 16) / 255.0f;
            }
            else
                TileSize = Vector3.One * 512;
        }

        public override string ToString() => $"Map3D:{MapId} {LogicalSize.X}x{LogicalSize.Y} TileSize: {TileSize}";
        public MapDataId MapId { get; }
        public Vector2 LogicalSize { get; }
        public Vector3 TileSize { get; }

        void Subscribed()
        {
            if (_skybox != null) Exchange.Attach(_skybox);
            if (_renderable != null) Exchange.Attach(_renderable);

            Raise(new SetClearColourEvent(_backgroundRed, _backgroundGreen, _backgroundBlue));
            //if(_labyrinthData.CameraHeight != 0)
            //    Debugger.Break();

            //if(_labyrinthData.Unk12 != 0) // 7=1|2|4 (Jirinaar), 54=32|16|4|2, 156=128|16|8|2 (Tall town)
            //    Debugger.Break();
            var maxObjectHeightRaw = _labyrinthData.ObjectGroups.Max(x => x.SubObjects.Max(y => (int?)y.Y));
            Raise(new LogEvent(1, $"WallHeight: {_labyrinthData.WallHeight} MaxObj: {maxObjectHeightRaw} EffWallWidth: {_labyrinthData.EffectiveWallWidth}"));
            Raise(new SetTileSizeEvent(TileSize, _labyrinthData.CameraHeight != 0 ? _labyrinthData.CameraHeight * 8 : TileSize.Y/2));

            float objectYScaling = TileSize.Y / _labyrinthData.WallHeight;
            if (maxObjectHeightRaw > _labyrinthData.WallHeight * 1.5f)
                objectYScaling /= 2; // TODO: Figure out the proper way to handle this.

            foreach (var npc in _mapData.Npcs)
            {
                var objectData = _labyrinthData.ObjectGroups[npc.ObjectNumber - 1];
                foreach (var subObject in objectData.SubObjects)
                {
                    var sprite = BuildSprite(npc.Waypoints[0].X, npc.Waypoints[0].Y, subObject, objectYScaling);
                    if (sprite == null)
                        continue;

                    Exchange.Attach(sprite);
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
                    }
                }
            }
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
            var tilePosition = new Vector3(tileX - 0.5f, 0, tileY - 0.5f) * TileSize;
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
