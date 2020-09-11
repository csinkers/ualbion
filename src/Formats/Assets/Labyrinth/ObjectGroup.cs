using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class ObjectGroup
    {
        public const int MaxSubObjectCount = 8;
        public ushort AutoGraphicsId { get; set; }
        SubObject[] _subObjects { get; } = new SubObject[MaxSubObjectCount];
        public IEnumerable<SubObject> SubObjects => _subObjects.Where(x => x != null);

        public override string ToString() =>
            $"Obj: AG{AutoGraphicsId} [ {string.Join("; ", SubObjects.Select(x => x.ToString()))} ]";

        public static ObjectGroup Serdes(int _, ObjectGroup og, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            og ??= new ObjectGroup();
            og.AutoGraphicsId = s.UInt16(nameof(og.AutoGraphicsId), og.AutoGraphicsId);


            for (int n = 0; n < MaxSubObjectCount; n++)
            {
                og._subObjects[n] ??= new SubObject();
                var so = og._subObjects[n];

                so.X = s.Int16(nameof(so.X), so.X);
                so.Z = s.Int16(nameof(so.Z), so.Z);
                so.Y = s.Int16(nameof(so.Y), so.Y);

                // so.ObjectInfoNumber = s.UInt16(nameof(so.ObjectInfoNumber), so.ObjectInfoNumber);

                so.ObjectInfoNumber = s.Transform<ushort, ushort>(
                   nameof(so.ObjectInfoNumber),
                    so.ObjectInfoNumber,
                    S.UInt16,
                    StoreIncrementedConverter.Instance);

                if (so.ObjectInfoNumber == 0xffff)
                    og._subObjects[n] = null;
            } // +64

            return og;
        }
    }
}
