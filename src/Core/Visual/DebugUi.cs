using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace UAlbion.Core.Visual;

public static class DebugUi
{
    public enum RenderableType
    {
        Line2D,
        Vector2D,
        Line3D,
        Vector3D,
        Sphere3D,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Renderable
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        public RenderableType Type;
        public CoordinateSystem System;
        public Vector3 A; // position or start point
        public Vector3 B; // end point, radius etc
        public Vector4 Color = Vector4.One; // RGBA color
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public Renderable(RenderableType type, CoordinateSystem system, Vector3 a, Vector3 b)
        {
            Type = type;
            System = system;
            A = a;
            B = b;
        }
    }

    public readonly struct Accessor
    {
        readonly int _index;
        public Accessor(int index) => _index = index;

        public Accessor Color(Vector3 color)
        {
            var span = CollectionsMarshal.AsSpan(RenderList);
            ref var r = ref span[_index];
            r.Color = new Vector4(r.Color.W);
            return this;
        }

        public Accessor Opacity(float alpha)
        {
            var span = CollectionsMarshal.AsSpan(RenderList);
            ref var r = ref span[_index];
            r.Color.W = alpha;
            return this;
        }
    }

    static readonly List<Renderable> RenderList = [];
    public static ReadOnlySpan<Renderable> GetSpan() => CollectionsMarshal.AsSpan(RenderList);
    public static void Clear() => RenderList.Clear();
    public static int Count => RenderList.Count;

    static Accessor Add(RenderableType type, CoordinateSystem system, Vector3 a, Vector3 b)
    {
        var index = RenderList.Count;
        RenderList.Add(new Renderable(RenderableType.Line2D, system, a, b));
        return new Accessor(index);
    }

    public static Accessor Line2D(CoordinateSystem coordinateSystem, Vector2 from, Vector2 to) =>
        Add(RenderableType.Line2D, coordinateSystem, new Vector3(from.X, from.Y, 0), new Vector3(to.X, to.Y, 0));

    public static Accessor Vector2D(CoordinateSystem coordinateSystem, Vector2 from, Vector2 to) =>
        Add(RenderableType.Vector2D, coordinateSystem, new Vector3(from.X, from.Y, 0), new Vector3(to.X, to.Y, 0));

    public static Accessor Line3D(CoordinateSystem coordinateSystem, Vector3 from, Vector3 to) =>
        Add(RenderableType.Line3D, coordinateSystem, from, to);

    public static Accessor Vector3D(CoordinateSystem coordinateSystem, Vector3 from, Vector3 to) =>
        Add(RenderableType.Vector3D, coordinateSystem, from, to);

    public static Accessor Sphere3D(CoordinateSystem coordinateSystem, Vector3 position, float radius) =>
        Add(RenderableType.Sphere3D, coordinateSystem, position, new Vector3(radius, 0, 0));
}