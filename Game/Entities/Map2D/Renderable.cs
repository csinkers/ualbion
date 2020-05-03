using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map2D
{
    public class Renderable : Component
    {
        readonly LogicalMap _logicalMap;
        readonly TileLayer _underlay;
        readonly TileLayer _overlay;

        public Renderable(LogicalMap logicalMap, ITexture tileset)
        {
            On<ToggleUnderlayEvent>(e => _underlay.IsActive = !_underlay.IsActive);
            On<ToggleOverlayEvent>(e => _overlay.IsActive = !_overlay.IsActive);

            _logicalMap = logicalMap;
            var subImage = tileset.GetSubImageDetails(0);
            TileSize = subImage.Size;

            _underlay = AttachChild(new TileLayer(
                logicalMap,
                tileset,
                logicalMap.GetUnderlay,
                DrawLayer.Underlay,
                IconChangeType.Underlay));

            _overlay = AttachChild(new TileLayer(logicalMap,
                tileset,
                logicalMap.GetOverlay,
                DrawLayer.Overlay,
                IconChangeType.Overlay));
        }

        public Vector2 TileSize { get; }
        public PaletteId Palette => _logicalMap.PaletteId;
        public Vector2 SizePixels => new Vector2(_logicalMap.Width, _logicalMap.Height) * TileSize;
        public WeakSpriteReference GetWeakUnderlayReference(int x, int y) => _underlay.GetWeakSpriteReference(x, y);
        public WeakSpriteReference GetWeakOverlayReference(int x, int y) => _overlay.GetWeakSpriteReference(x, y);

        public int? HighlightIndex
        {
            set
            {
                _underlay.HighlightIndex = value;
                _overlay.HighlightIndex = value;
            }
        }

        protected override void Subscribed() => Raise(new LoadPaletteEvent(Palette));
    }
}
