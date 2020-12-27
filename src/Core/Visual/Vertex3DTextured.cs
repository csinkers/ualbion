using System;
using System.Numerics;

namespace UAlbion.Core.Visual
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    // ReSharper disable CompareOfFloatsByEqualityOperator
    public struct Vertex3DTextured : IEquatable<Vertex3DTextured>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float U;
        public readonly float V;

        public Vertex3DTextured(Vector3 position, Vector2 textureCoordinates)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            U = textureCoordinates.X;
            V = textureCoordinates.Y;
        }

        public Vertex3DTextured(float x, float y, float z, float u, float v) { X = x; Y = y; Z = z; U = u; V = v; }
        public override bool Equals(object obj) => obj is Vertex3DTextured other && Equals(other);
        public bool Equals(Vertex3DTextured other) => X == other.X && Y == other.Y && Z == other.Z && U == other.U && V == other.V;
        public override int GetHashCode() => HashCode.Combine(X, Y, Z, U, V);
        public static bool operator ==(Vertex3DTextured left, Vertex3DTextured right) => left.Equals(right);
        public static bool operator !=(Vertex3DTextured left, Vertex3DTextured right) => !(left == right);
    }
    // ReSharper restore CompareOfFloatsByEqualityOperator
#pragma warning restore CA1051 // Do not declare visible instance fields
}
