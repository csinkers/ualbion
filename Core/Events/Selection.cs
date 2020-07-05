using System.Numerics;

namespace UAlbion.Core.Events
{
    public struct Selection
    {
        public Selection(Vector3 intersectionPoint, float distance, object target)
        {
            IntersectionPoint = intersectionPoint;
            Target = target;
            Distance = distance;
        }

        public Selection(Vector3 rayOrigin, Vector3 rayDirection, float distance, object target)
            : this(rayOrigin + distance * rayDirection, distance, target) { }

        public Vector3 IntersectionPoint { get; }
        public float Distance { get; }
        public object Target { get; }
        public override string ToString() => $"{Target} @ {IntersectionPoint} ({Distance})";
    }
}
