using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Objects;
using UAlbion.Core.Textures;
using UAlbion.Formats.Parsers;
using UAlbion.Game.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class Map : Component
    {
        readonly SpriteRenderer.InstanceData _blankInstance = new SpriteRenderer.InstanceData(Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, 0, 0);
        readonly Scene _scene;
        readonly Map2D _map;
        readonly bool _useSmallSprites;
        readonly SpriteRenderer.MultiSprite _underlay;
        readonly SpriteRenderer.MultiSprite _overlay;
        int _frameCount;
        bool _subscribed;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Map, RenderEvent>((x, e) => x.Render(e)),
            new Handler<Map, UpdateEvent>((x, e) => x.Update(e)),
            new Handler<Map, SubscribedEvent>((x, e) => x.Subscribed())
        };

        readonly ITexture _tileset;
        readonly TilesetData _tileData;
        readonly Vector2 _tileSize;

        void Subscribed()
        {
            if (_subscribed) return;

            Raise(new LoadPalEvent((int)GetPalette()));
            foreach (var npc in _map.Npcs)
            {
                IComponent sprite = 
                    _useSmallSprites
                    ? (IComponent)new SmallNpcSprite((SmallNpcId)npc.ObjectNumber, npc.Waypoints)
                    : new LargeNpcSprite((LargeNpcId)npc.ObjectNumber, npc.Waypoints);

                _scene.AddComponent(sprite);
            }

            _subscribed = true;
        }
        SpriteRenderer.InstanceData BuildInstanceData(int i, int j, TilesetData.TileData tile, int tickCount, bool isOverlay)
        {
            int underlayId = tile.GetSubImageForTile(tickCount);
            if (underlayId == ushort.MaxValue)
                return _blankInstance;

            _tileset.GetSubImageDetails(underlayId, out var tileSize, out var texPosition, out var texSize, out var layer);
            var instance = new SpriteRenderer.InstanceData();
            instance.Offset = new Vector2(i, j) * tileSize;
            instance.Size = tileSize;

            instance.TexPosition = texPosition;
            instance.TexSize = texSize;
            instance.TexLayer = layer;

            //if (!isOverlay)
            {
                instance.Flags =
                    (_tileset is EightBitTexture ? SpriteFlags.UsePalette : 0)
                    | ((tile.Flags & TilesetData.TileFlags.Unk5) != 0 ? SpriteFlags.RedTint : 0)
                    //| (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
                    //| (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
                    //| (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
                    ;
            }
            //else instance.Flags = _tileset is EightBitTexture ? SpriteFlags.UsePalette : 0;

            return instance;
        }

        public Map(Assets assets, Scene scene, MapDataId mapId) : base(Handlers)
        {
            _scene = scene;
            _map = assets.LoadMap2D(mapId);
            _tileset = assets.LoadTexture((IconGraphicsId)_map.TilesetId);
            _tileData = assets.LoadTileData((IconDataId)_map.TilesetId);
            _useSmallSprites = _tileData.UseSmallGraphics;
            _tileSize = BuildInstanceData(0, 0, _tileData.Get(0), 0, false).Size;

            var underlay = new List<SpriteRenderer.InstanceData>();
            var overlay = new List<SpriteRenderer.InstanceData>();
            for (int j = 0; j < _map.Height; j++)
            {
                for (int i = 0; i < _map.Width; i++)
                {
                    var underlayTile = _tileData.Get(_map.Underlay[j * _map.Width + i]);
                    underlay.Add(BuildInstanceData(i, j, underlayTile, 0, false));

                    var overlayTile = _tileData.Get(_map.Overlay[j * _map.Width + i]);
                    overlay.Add(BuildInstanceData(i, j, overlayTile, 0, true));
                }
            }

            _underlay = new SpriteRenderer.MultiSprite(new SpriteRenderer.SpriteKey(_tileset, (int) DrawLayer.Underlay)) { Instances = underlay.ToArray() };
            _overlay = new SpriteRenderer.MultiSprite(new SpriteRenderer.SpriteKey(_tileset, (int)DrawLayer.Overlay)) { Instances = overlay.ToArray() };
        }

        void Update(UpdateEvent updateEvent)
        {
            int underlayIndex = 0;
            int overlayIndex = 0;
            for (int j = 0; j < _map.Height; j++)
            {
                for (int i = 0; i < _map.Width; i++)
                {
                    var underlayTile = _tileData.Get(_map.Underlay[j * _map.Width + i]);
                    _underlay.Instances[underlayIndex] = BuildInstanceData(i, j, underlayTile, _frameCount, false);
                    underlayIndex++;

                    var overlayTile = _tileData.Get(_map.Overlay[j * _map.Width + i]);
                    _overlay.Instances[overlayIndex] = BuildInstanceData(i, j, overlayTile, _frameCount, true);
                    overlayIndex++;
                }
            }

            _frameCount++;
        }

        public PaletteId GetPalette() => (PaletteId)_map.PaletteId;
        public Vector2 Size => new Vector2(_map.Width, _map.Height) * _tileSize;

        void Render(RenderEvent e)
        {
            e.Add(_underlay);
            e.Add(_overlay);
        }
    }
}