using System.Numerics;

namespace UAlbion.Core
{
    public interface IPositioned
    {
        Vector3 Position { get; }
        Vector3 Dimensions { get; }
        (float, Vector3)? RayIntersect(Vector3 origin, Vector3 direction); // Return intersection distance and location.
    }
}