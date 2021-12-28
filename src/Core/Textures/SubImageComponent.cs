using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures;

class SubImageComponent
{
    public ITexture Texture { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int? W { get; set; }
    public int? H { get; set; }
    public byte Alpha { get; set; } = 0xff;
    public override string ToString() => $"({X}, {Y}) {Texture}";
}