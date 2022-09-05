using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid;

public static class RayExtensions
{
    public static bool Intersects(this Ray ray, ref BoundingBox box, out float tmin)
    {
        // http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection

        tmin = (box.Min.X - ray.Origin.X) / ray.Direction.X;
        float tmax = (box.Max.X - ray.Origin.X) / ray.Direction.X;

        if (tmin > tmax)
            Swap(ref tmin, ref tmax);

        float tymin = (box.Min.Y - ray.Origin.Y) / ray.Direction.Y;
        float tymax = (box.Max.Y - ray.Origin.Y) / ray.Direction.Y;

        if (tymin > tymax)
            Swap(ref tymin, ref tymax);

        if (tmin > tymax || tymin > tmax)
            return false;

        if (tymin > tmin)
            tmin = tymin;

        if (tymax < tmax)
            tmax = tymax;

        float tzmin = (box.Min.Z - ray.Origin.Z) / ray.Direction.Z;
        float tzmax = (box.Max.Z - ray.Origin.Z) / ray.Direction.Z;

        if (tzmin > tzmax)
            Swap(ref tzmin, ref tzmax);

        if (tmin > tzmax || tzmin > tmax)
            return false;

        if (tzmin > tmin)
            tmin = tzmin;

        // if (tzmax < tmax)
        //     tmax = tzmax;

        return true;
    }
    static void Swap(ref float a, ref float b) => (a, b) = (b, a);
}