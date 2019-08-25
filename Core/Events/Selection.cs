using System.Numerics;

namespace UAlbion.Core.Events
{
    public class Selection
    {
        public Selection(Vector3 intersectionPoint, string name, object target)
        {
            IntersectionPoint = intersectionPoint;
            Name = name;
            Target = target;
        }

        public Vector3 IntersectionPoint { get; }
        public string Name { get; }
        public object Target { get; }
    }
}