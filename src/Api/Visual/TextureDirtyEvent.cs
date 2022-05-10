using UAlbion.Api.Eventing;

namespace UAlbion.Api.Visual;

public record TextureDirtyEvent(ITexture Texture) : IVerboseEvent
{
    public string ToStringNumeric() => ToString();
}