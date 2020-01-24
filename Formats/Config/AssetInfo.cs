using System.Collections.Generic;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public abstract class AssetInfo
    {
        [JsonIgnore] public int Id;
        [JsonIgnore] public abstract FileFormat Format { get; }
        [JsonIgnore] public abstract int EffectiveWidth { get; }
        [JsonIgnore] public abstract int EffectiveHeight { get; }
        [JsonIgnore] public abstract bool Transposed { get; }

        public int? Width;
        public int? Height;
        public string SubSprites;
        public bool? UseSmallGraphics;
    }

    public class BasicAssetInfo : AssetInfo
    {
        public BasicAssetInfo() { }
        public BasicAssetInfo(FullAssetInfo asset)
        {
            Id = asset.Id;
            Width = asset.Width;
            Height = asset.Height;
            SubSprites = asset.SubSprites;
            UseSmallGraphics = asset.UseSmallGraphics;
        }

        [JsonIgnore] public XldInfo Parent { get; internal set; }
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
        public string Name;
        public IList<int> PaletteHints;
        [JsonIgnore] public FullXldInfo Parent;
        [JsonIgnore] public override FileFormat Format => Parent.Format;
        [JsonIgnore] public override int EffectiveWidth => Width ?? Parent.Width ?? 0;
        [JsonIgnore] public override int EffectiveHeight => Height ?? Parent.Height ?? 0;
        [JsonIgnore] public override bool Transposed => Parent.Transposed ?? false;
    }
}