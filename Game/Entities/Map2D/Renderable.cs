using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map2D
{
    public class Renderable : Component
    {
        readonly LogicalMap _logicalMap;
        readonly TileLayer _underlay;
        readonly TileLayer _overlay;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Renderable, ToggleUnderlayEvent>((x,e) => x._underlay.IsActive = !x._underlay.IsActive),
            H<Renderable, ToggleOverlayEvent>((x,e) => x._overlay.IsActive = !x._overlay.IsActive)
        );

        public Vector2 TileSize { get; }
        public PaletteId Palette => _logicalMap.PaletteId;
        public Vector2 SizePixels => new Vector2(_logicalMap.Width, _logicalMap.Height) * TileSize;

        public int? HighlightIndex
        {
            set
            {
                _underlay.HighlightIndex = value;
                _overlay.HighlightIndex = value;
            }
        }

        public Renderable(LogicalMap logicalMap, ITexture tileset) : base(Handlers)
        {
            _logicalMap = logicalMap;
            tileset.GetSubImageDetails(0, out var tileSize, out _, out _, out _);
            TileSize = tileSize;
            _underlay = new TileLayer(logicalMap, tileset, logicalMap.GetUnderlay, DrawLayer.Underlay);
            _overlay = new TileLayer(logicalMap, tileset, logicalMap.GetOverlay, DrawLayer.Overlay3);
            Children.Add(_underlay);
            Children.Add(_overlay);
        }

        public override void Subscribed() => Raise(new LoadPaletteEvent(Palette));
    }
}
