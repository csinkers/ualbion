using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public abstract class AssetInfo
    {
        [JsonIgnore] public int Id { get; set;  }
        [JsonIgnore] public abstract FileFormat Format { get; }
        [JsonIgnore] public abstract int EffectiveWidth { get; }
        [JsonIgnore] public abstract int EffectiveHeight { get; }
        [JsonIgnore] public abstract bool Transposed { get; }

        public int? Width { get; set; }
        public int? Height { get; set; }
        public string SubSprites { get; set; }
        public bool? UseSmallGraphics { get; set; }
    }

    public class BasicAssetInfo : AssetInfo
    {
        public BasicAssetInfo() { }
        public BasicAssetInfo(FullAssetInfo asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            Id = asset.Id;
            Width = asset.Width;
            Height = asset.Height;
            SubSprites = asset.SubSprites;
            UseSmallGraphics = asset.UseSmallGraphics;
        }

        [JsonIgnore] public AssetFileInfo Parent { get; internal set; }
        [JsonIgnore] public override FileFormat Format => Parent.Format;
        [JsonIgnore] public override int EffectiveWidth => Width ?? Parent.Width ?? 0;
        [JsonIgnore] public override int EffectiveHeight => Height ?? Parent.Height ?? 0;
        [JsonIgnore] public override bool Transposed => Parent.Transposed ?? false;
        [JsonIgnore] public bool ContainsData =>
            Width != null ||
            Height != null ||
            !string.IsNullOrEmpty(SubSprites)  ||
            UseSmallGraphics != null;
    }

    public class FullAssetInfo : AssetInfo
    {
        public string Name { get; set; }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Play nicely with JSON serialisation")]
        public IList<int> PaletteHints { get; set; }
        [JsonIgnore] public FullAssetFileInfo Parent { get; set; }
        [JsonIgnore] public override FileFormat Format => Parent.Format;
        [JsonIgnore] public override int EffectiveWidth => Width ?? Parent.Width ?? 0;
        [JsonIgnore] public override int EffectiveHeight => Height ?? Parent.Height ?? 0;
        [JsonIgnore] public override bool Transposed => Parent.Transposed ?? false;
        [JsonIgnore] public string Filename { get; set; }
    }
}
