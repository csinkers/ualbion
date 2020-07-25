using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;

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
            s.Begin();
            og.AutoGraphicsId = s.UInt16(nameof(og.AutoGraphicsId), og.AutoGraphicsId);

            for (int n = 0; n < 8; n++)
            {
                var so = og.SubObjects.Count <= n
                    ? new SubObject()
                    : og.SubObjects[n];

                if (og.SubObjects.Count > n)
                    so.ObjectInfoNumber++;

                so.X = s.Int16(nameof(so.X), so.X);
                so.Z = s.Int16(nameof(so.Z), so.Z);
                so.Y = s.Int16(nameof(so.Y), so.Y);

                so.ObjectInfoNumber = s.UInt16(nameof(so.ObjectInfoNumber), so.ObjectInfoNumber);
                if(s.Mode != SerializerMode.Reading)
                    throw new NotImplementedException();

                if (so.ObjectInfoNumber != 0)
                {
                    so.ObjectInfoNumber--;
                    if (og.SubObjects.Count <= n)
                        og.SubObjects.Add(so);
                }
            } // +64

            s.End();
            return og;
        }
    }
}
