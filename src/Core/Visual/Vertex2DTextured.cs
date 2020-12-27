using System;
using System.Numerics;

namespace UAlbion.Core.Visual
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    public struct Vertex2DTextured : IEquatable<Vertex2DTextured>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float U;
        public readonly float V;

        public Vertex2DTextured(Vector2 position, Vector2 textureCoordinates)
        {
            X = position.X;
            Y = position.Y;
            U = textureCoordinates.X;
            V = textureCoordinates.Y;
        }

        public Vertex2DTextured(float x, float y, float u, float v) { X = x; Y = y; U = u; V = v; }

        public override bool Equals(object obj) => obj is Vertex2DTextured other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, U, V);
        public static bool operator ==(Vertex2DTextured left, Vertex2DTextured right) => left.Equals(right);
        public static bool operator !=(Vertex2DTextured left, Vertex2DTextured right) => !(left == right);
        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool Equals(Vertex2DTextured other) => X == other.X && Y == other.Y && U == other.U && V == other.V;
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
