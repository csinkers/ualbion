using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Visual;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class FontDefinition
{
    readonly Dictionary<char, (int regionNumber, int advance)> _charInfo = new();
    readonly List<(IReadOnlyTexture<byte>, Region)> _sourceRegions = new();
    readonly List<Region> _destRegions = new();
    SpriteSheetLayout _layout;

    public static uint[] ExportPalette { get; } = BuildExportPalette();
    static uint[] BuildExportPalette()
    {
        var result = new uint[256];
        result[1] = 0xffffffff;
        result[2] = 0xffcccccc;
        result[3] = 0xffaaaaaa;
        result[4] = 0xff777777;
        result[5] = 0xff555555;
        return result;
    }

    public int LineHeight { get; set; } // Size in UI pixels
    public int SpaceSize { get; set; } = 3; // Advance for a whitespace character
    public int Margin { get; set; } = 1; // Default whitespace pixels to include in advance when not overriden by kerning
    public List<FontComponent> Components { get; set; } = new();
    public Dictionary<char, Dictionary<char, int>> Kerning { get; set; } = new();

    public bool SupportsCharacter(char c) => _charInfo.ContainsKey(c);
    public int GetAdvance(char c, char nextChar)
    {
        if (Kerning.TryGetValue(c, out var letterKerning) && letterKerning.TryGetValue(nextChar, out var advance))
            return advance;

        return _charInfo.TryGetValue(c, out var value) ? value.advance : SpaceSize;
    }

    public Region GetRegion(char c) =>
        _charInfo.TryGetValue(c, out var value)
            ? _destRegions[value.regionNumber]
            : null;

    void PopulateRegions(FontComponent component, IReadOnlyTexture<byte> texture)
    {
        foreach (var c in component.Mapping)
        {
            var region = component.TryGetRegion(c, texture);
            if (region == null)
                continue;

            var buffer = texture.GetRegionBuffer(region);
            int width = 0;
            for (int j = 0; j < buffer.Height; j++)
            {
                var row = buffer.GetRow(j);
                for (int i = 0; i < row.Length; i++)
                    if (row[i] != 0 && i > width)
                        width = i;
            }
            width++;

            var trimmed = new Region(region.X, region.Y, width, region.Height, texture.Width, texture.Height, region.Layer);
            _charInfo[c] = (_sourceRegions.Count, width + Margin);
            _sourceRegions.Add((texture, trimmed));
        }

        _layout = SpriteSheetUtil.ArrangeSpriteSheet(_sourceRegions.Count, 1, x => _sourceRegions[x].Item1.GetRegionBuffer(_sourceRegions[x].Item2));
        for (int i = 0; i < _sourceRegions.Count; i++)
        {
            var pos = _layout.Positions[i];
            var srcRegion = _sourceRegions[i].Item2;
            _destRegions.Add(new Region(pos.X, pos.Y, srcRegion.Width, srcRegion.Height, _layout.Width, _layout.Height, pos.Layer));
        }
    }

    public MetaFont Build(FontId fontId, InkId inkId, IAssetManager assets)
    {
        ArgumentNullException.ThrowIfNull(assets);
        if (_layout == null)
        {
            foreach (var component in Components)
            {
                var texture = (IReadOnlyTexture<byte>)assets.LoadTexture(component.GraphicsId);
                if (texture == null)
                    continue;

                PopulateRegions(component, texture);
            }

            if (_layout == null)
                throw new InvalidOperationException($"Could not build sprite-sheet layout for font {fontId}");
        }

        var metaId = new MetaFontId(fontId, inkId);

        var ink = assets.LoadInk(inkId);
        if (ink == null)
            throw new ArgumentException($"Ink color {inkId} not found", nameof(inkId));

        var pixelData = new byte[_layout.Width * _layout.Height * _layout.Layers];

        var mapping = ink.PaletteMapping.Select(x => (byte)x).ToArray();
        for (int i = 0; i < _sourceRegions.Count; i++)
        {
            var srcBuffer = _sourceRegions[i].Item1.GetRegionBuffer(_sourceRegions[i].Item2);
            var dr = _destRegions[i];
            var destSlice = pixelData.AsSpan().Slice(dr.PixelOffset, dr.PixelLength);
            var destBuffer = new ImageBuffer<byte>(dr.Width, dr.Height, _layout.Width, destSlice);
            BlitUtil.Blit8Translated(srcBuffer, destBuffer, mapping);
        }

        IMutableTexture<byte> resultTexture = _layout.Layers > 1
            ? new ArrayTexture<byte>(metaId, $"Font{metaId}", _layout.Width, _layout.Height, _layout.Layers, pixelData, _destRegions)
            : new SimpleTexture<byte>(metaId, $"Font{metaId}", _layout.Width, _layout.Height, pixelData, _destRegions);

        return new MetaFont(metaId, this, resultTexture);
    }
}