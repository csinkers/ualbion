using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapRenderable2D : Component
    {
        readonly MapData2D _mapData;
        readonly TileLayer _underlay;
        readonly TileLayer _overlay;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapRenderable2D, ToggleUnderlayEvent>((x,e) => x._underlay.IsActive = !x._underlay.IsActive),
            H<MapRenderable2D, ToggleOverlayEvent>((x,e) => x._overlay.IsActive = !x._overlay.IsActive)
        );

        public Vector2 TileSize { get; }
        public PaletteId Palette => (PaletteId)_mapData.PaletteId;
        public Vector2 SizePixels => new Vector2(_mapData.Width, _mapData.Height) * TileSize;

        public int? HighlightIndex
        {
            set
            {
                _underlay.HighlightIndex = value;
                _overlay.HighlightIndex = value;
            }
        }

        public MapRenderable2D(MapData2D mapData, ITexture tileset, TilesetData tileData) : base(Handlers)
        {
            _mapData = mapData;
            tileset.GetSubImageDetails(0, out var tileSize, out _, out _, out _);
            TileSize = tileSize;
            _underlay = new TileLayer(mapData, tileData, tileset, mapData.Underlay, DrawLayer.Underlay);
            _overlay = new TileLayer(mapData, tileData, tileset, mapData.Overlay, DrawLayer.Overlay3);
            Children.Add(_underlay);
            Children.Add(_overlay);
        }

        public override void Subscribed() => Raise(new LoadPaletteEvent(Palette));
    }
}
