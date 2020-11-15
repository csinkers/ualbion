using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class AssetInfo
    {
        [JsonIgnore] public AssetFileInfo Parent { get; internal set; }
        [JsonIgnore] public FileFormat Format => Parent.Format;
        [JsonIgnore] public int EffectiveWidth => Width ?? Parent.Width ?? 0;
        [JsonIgnore] public int EffectiveHeight => Height ?? Parent.Height ?? 0;
        [JsonIgnore] public bool Transposed => Parent.Transposed ?? false;
        [JsonIgnore] public string Filename { get; set; }

        public string Name { get; set; }
        public int Id { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string SubSprites { get; set; }
        public bool? UseSmallGraphics { get; set; }
        public long? Offset { get; set; }
        public Position2D Hotspot { get; set; }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Play nicely with JSON serialisation")]
        public IList<int> PaletteHints { get; set; }
    }
}
