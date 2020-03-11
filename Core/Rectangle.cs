using System;
using System.Numerics;

namespace UAlbion.Core
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public int X;

        public int Y;

        public int Width;

        public int Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Point topLeft, Point size)
        {
            X = topLeft.X;
            Y = topLeft.Y;
            Width = size.X;
            Height = size.Y;
        }

        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;

        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        public bool Contains(Point p) => Contains(p.X, p.Y);
        public bool Contains(int x, int y)
        {
            return (X <= x && (X + Width) > x)
                && (Y <= y && (Y + Height) > y);
        }

        public override string ToString() => $"({Left}, {Top}, {Right}, {Bottom})";
        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);
        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);
        public bool Equals(Rectangle other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is Rectangle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    }
}
