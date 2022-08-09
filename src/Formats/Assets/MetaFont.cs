using System;
using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets;

public class MetaFont
{
    public MetaFont(MetaFontId id, FontDefinition definition, ITexture texture)
    {
        Id = id;
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
    }

    public MetaFontId Id { get; }
    public FontDefinition Definition { get; }
    public ITexture Texture { get; }
    public int GetAdvance(char c, char nextChar) => Definition.GetAdvance(c, nextChar);
    public Region GetRegion(char c) => Definition.GetRegion(c);
    public bool SupportsCharacter(char c) => Definition.SupportsCharacter(c);
}