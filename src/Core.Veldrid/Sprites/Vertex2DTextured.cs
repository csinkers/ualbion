﻿using System.Numerics;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
    public readonly partial struct Vertex2DTextured : IVertexFormat
    {
        [Vertex("Position")] public Vector2 Position { get; }
        [Vertex("TexCoords")] public Vector2 Texture { get; }

        public Vertex2DTextured(float x, float y, float u, float v)
        {
            Position = new Vector2(x, y);
            Texture = new Vector2(u, v);
        }
    }
}