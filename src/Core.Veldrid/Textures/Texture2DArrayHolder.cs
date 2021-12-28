using System;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Textures;

public sealed class Texture2DArrayHolder : TextureHolder, ITextureArrayHolder
{
    public Texture2DArrayHolder(string name) : base(name) { }
    protected override void Validate(Texture texture)
    {
        if (texture == null)
            return;
        if (texture.Type != TextureType.Texture2D)
            throw new ArgumentOutOfRangeException($"Tried to assign a {texture.Type} to Texture2DArrayHolder \"{Name}\"");
        if (texture.ArrayLayers < 2)
            throw new ArgumentOutOfRangeException($"Tried to assign a single-layer texture to Texture2DArrayHolder \"{Name}\"");
    }
}