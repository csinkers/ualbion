using System;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Textures;

public sealed class Texture2DHolder : TextureHolder, ITextureHolder
{
    public Texture2DHolder(string name) : base(name) { }
    protected override void Validate(Texture texture)
    {
        if (texture == null)
            return;
        if (texture.Type != TextureType.Texture2D)
            throw new ArgumentOutOfRangeException($"Tried to assign a {texture.Type} to Texture2DHolder \"{Name}\"");
        if (texture.ArrayLayers > 1)
            throw new ArgumentOutOfRangeException($"Tried to assign a multi-layer texture to Texture2DHolder \"{Name}\"");
    }
}