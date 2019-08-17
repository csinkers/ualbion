using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Objects;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapRenderable2D : Component
    {
        readonly SpriteRenderer.InstanceData _blankInstance = new SpriteRenderer.InstanceData(
            Vector2.Zero, Vector2.Zero, 
            Vector2.Zero, Vector2.Zero, 0, 0);

        readonly MapData2D _mapData;
        readonly ITexture _tileset;
        readonly TilesetData _tileData;
        readonly SpriteRenderer.MultiSprite _underlay;
        readonly SpriteRenderer.MultiSprite _overlay;
        int _frameCount;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MapRenderable2D, RenderEvent>((x, e) => x.Render(e)),
            new Handler<MapRenderable2D, UpdateEvent>((x, e) => x.Update()),
            new Handler<MapRenderable2D, SubscribedEvent>((x, e) => x.Subscribed())
        };

        public Vector2 TileSize { get; }
        public PaletteId Palette => (PaletteId)_mapData.PaletteId;
        public Vector2 SizePixels => new Vector2(_mapData.Width, _mapData.Height) * TileSize;
        public int? HighlightIndex { get; set; }
        int? _highLightEvent;

        public MapRenderable2D(MapData2D mapData, ITexture tileset, TilesetData tileData) : base(Handlers)
        {
            _mapData = mapData;
            _tileset = tileset;
            _tileData = tileData;
            TileSize = BuildInstanceData(0, 0, _tileData.Tiles[0], 0, false).Size;

            var underlay = new List<SpriteRenderer.InstanceData>();
            var overlay = new List<SpriteRenderer.InstanceData>();
            for (int j = 0; j < _mapData.Height; j++)
            {
                for (int i = 0; i < _mapData.Width; i++)
                {
                    var underlayTile = _tileData.Tiles[_mapData.Underlay[j * _mapData.Width + i]];
                    underlay.Add(BuildInstanceData(i, j, underlayTile, 0, false));

                    var overlayTile = _tileData.Tiles[_mapData.Overlay[j * _mapData.Width + i]];
                    overlay.Add(BuildInstanceData(i, j, overlayTile, 0, true));
                }
            }

            _underlay = new SpriteRenderer.MultiSprite(new SpriteRenderer.SpriteKey(_tileset, (int) DrawLayer.Underlay)) { Instances = underlay.ToArray() };
            _overlay = new SpriteRenderer.MultiSprite(new SpriteRenderer.SpriteKey(_tileset, (int)DrawLayer.Overlay)) { Instances = overlay.ToArray() };
        }

        void Subscribed()
        {
            Raise(new LoadPalEvent((int)Palette));
        }

        SpriteRenderer.InstanceData BuildInstanceData(int i, int j, TilesetData.TileData tile, int tickCount, bool isOverlay)
        {
            int index = j * _mapData.Width + i;
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

            int zoneNum = _mapData.ZoneLookup[index];
            int eventNum = zoneNum == -1 ? -1 : _mapData.Zones[zoneNum].EventNumber;

            instance.Flags =
                (_tileset is EightBitTexture ? SpriteFlags.UsePalette : 0)
                | (HighlightIndex == index ? SpriteFlags.Highlight : 0)
                | (eventNum != -1 && _highLightEvent != eventNum ? SpriteFlags.Highlight : 0)
                | (_highLightEvent == eventNum ? SpriteFlags.GreenTint : 0)
                //| ((tile.Flags & TilesetData.TileFlags.TextId) != 0 ? SpriteFlags.RedTint : 0)
                //| (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
                //| (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
                //| (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
                ;

            return instance;
        }

        void Update()
        {
            if(HighlightIndex.HasValue)
            {
                int zoneNum = _mapData.ZoneLookup[HighlightIndex.Value];
                _highLightEvent = zoneNum == -1 ? -1 : _mapData.Zones[zoneNum].EventNumber;
                if (_highLightEvent == -1)
                    _highLightEvent = null;
            }
            else _highLightEvent = null;

            int underlayIndex = 0;
            int overlayIndex = 0;
            for (int j = 0; j < _mapData.Height; j++)
            {
                for (int i = 0; i < _mapData.Width; i++)
                {
                    var underlayTile = _tileData.Tiles[_mapData.Underlay[j * _mapData.Width + i]];
                    _underlay.Instances[underlayIndex] = BuildInstanceData(i, j, underlayTile, 3 * _frameCount / 2, false);
                    underlayIndex++;

                    var overlayTile = _tileData.Tiles[_mapData.Overlay[j * _mapData.Width + i]];
                    _overlay.Instances[overlayIndex] = BuildInstanceData(i, j, overlayTile, 3 * _frameCount / 2, true);
                    overlayIndex++;
                }
            }

            _frameCount++;
        }

        void Render(RenderEvent e)
        {
            e.Add(_underlay);
            e.Add(_overlay);
        }
    }
}