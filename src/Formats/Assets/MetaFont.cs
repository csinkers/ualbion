using System;
using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets;

public class MetaFont : IFont
{
    public MetaFont(MetaFontId id, FontDefinition definition, IReadOnlyTexture<byte> texture)
    {
        Id = id;
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
    }

    public MetaFontId Id { get; }
    public FontDefinition Definition { get; }
    public IReadOnlyTexture<byte> Texture { get; }
    public int GetAdvance(char c, char nextChar) => Definition.GetAdvance(c, nextChar);
    public Region GetRegion(char c) => Definition.GetRegion(c);
    public ReadOnlyImageBuffer<byte> GetRegionBuffer(char c) => Texture.GetRegionBuffer(Definition.GetRegion(c));
    public bool SupportsCharacter(char c) => Definition.SupportsCharacter(c);
}