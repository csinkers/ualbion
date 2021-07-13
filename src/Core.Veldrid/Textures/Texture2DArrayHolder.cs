using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Textures
{
    public sealed class Texture2DArrayHolder : TextureHolder, ITextureArrayHolder
    {
        public Texture2DArrayHolder(string name) : base(name) { }
    }
}
