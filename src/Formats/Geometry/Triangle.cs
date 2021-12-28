using System;
using System.Numerics;

namespace UAlbion.Formats.Geometry;

public readonly struct Triangle : IEquatable<Triangle>
{
    public Triangle(Vector2 a, Vector2 b, Vector2 c) { A = a; B = b; C = c; }
    public Vector2 A { get; }
    public Vector2 B { get; }
    public Vector2 C { get; }

    public static bool operator ==(Triangle a, Triangle b) => a.Equals(b);
    public static bool operator !=(Triangle a, Triangle b) => !a.Equals(b);
    public bool Equals(Triangle other) => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
    public override bool Equals(object obj) => obj is Triangle other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = A.GetHashCode();
            hashCode = (hashCode * 397) ^ B.GetHashCode();
            hashCode = (hashCode * 397) ^ C.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Calculates the circumcircle of the triangle.
    /// </summary>
    /// <returns>The circumcircle centre position and squared radius</returns>
    public (Vector2, float) Circumcircle()
    {
        var b = B - A;
        var c = C - A;

        var d = 2 * (b.X * c.Y - b.Y * c.X);
        var lenB2 = b.X * b.X + b.Y * b.Y;
        var lenC2 = c.X * c.X + c.Y * c.Y;
        var ux = (c.Y * lenB2 - b.Y * lenC2) / d;
        var uy = (b.X * lenC2 - c.X * lenB2) / d;
        var radius2 = ux * ux + uy * uy;
        var centre = new Vector2(ux, uy) + A;
        return (centre, radius2);
    }
}