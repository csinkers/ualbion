using System;
using System.Numerics;

namespace UAlbion.Core.Events
{
    public struct Selection : IEquatable<Selection>
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
        public override int GetHashCode() => HashCode.Combine(IntersectionPoint, Distance, Target);
        public static bool operator ==(Selection left, Selection right) => left.Equals(right);
        public static bool operator !=(Selection left, Selection right) => !(left == right);
        public override bool Equals(object obj) => obj is Selection other && Equals(other);
        public bool Equals(Selection other)
            => IntersectionPoint.Equals(other.IntersectionPoint) &&
               Math.Abs(Distance - other.Distance) < float.Epsilon &&
               Target == other.Target;
    }
}
