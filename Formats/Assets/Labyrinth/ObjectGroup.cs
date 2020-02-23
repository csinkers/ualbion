using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class ObjectGroup
    {
        public ushort AutoGraphicsId { get; set; }
        public IList<SubObject> SubObjects { get; } = new List<SubObject>();

        public override string ToString() =>
            $"Obj: AG{AutoGraphicsId} [ {string.Join("; ", SubObjects.Select(x => x.ToString()))} ]";

        public static ObjectGroup Serdes(int _, ObjectGroup og, ISerializer s)
        {
            og ??= new ObjectGroup();
            s.Dynamic(og, nameof(og.AutoGraphicsId));

            for (int n = 0; n < 8; n++)
            {
                var so = og.SubObjects.Count <= n
                    ? new SubObject()
                    : og.SubObjects[n];

                if (og.SubObjects.Count > n)
                    so.ObjectInfoNumber++;

                s.Dynamic(so, nameof(so.X));
                s.Dynamic(so, nameof(so.Z));
                s.Dynamic(so, nameof(so.Y));
                s.Dynamic(so, nameof(so.ObjectInfoNumber));

                if (so.ObjectInfoNumber != 0)
                {
                    so.ObjectInfoNumber--;
                    if (og.SubObjects.Count <= n)
                        og.SubObjects.Add(so);
                }
            } // +64

            return og;
        }
    }
}