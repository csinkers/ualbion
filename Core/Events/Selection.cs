using System.Numerics;

namespace UAlbion.Core.Events
{
    public class Selection
    {
        public Selection(Vector3 intersectionPoint, object target)
        {
            IntersectionPoint = intersectionPoint;
            Target = target;
        }

        public Vector3 IntersectionPoint { get; }
        public object Target { get; }
    }
}