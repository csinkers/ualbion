using System;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;

namespace UAlbion.Formats.Assets.Labyrinth;

public class ObjectGroup
{
    public const int MaxSubObjectCount = 8;
    public ushort AutoGraphicsId { get; set; }
    [JsonInclude] public SubObject[] SubObjects { get; private set; } = new SubObject[MaxSubObjectCount];

    public override string ToString() =>
        $"Obj: AG{AutoGraphicsId} [ {string.Join("; ", SubObjects.Select(x => x.ToString()))} ]";

    public static ObjectGroup Serdes(int _, ObjectGroup og, ISerializer s) // total size 0x42 = 66
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        og ??= new ObjectGroup();
        og.AutoGraphicsId = s.UInt16(nameof(og.AutoGraphicsId), og.AutoGraphicsId);

        for (int n = 0; n < MaxSubObjectCount; n++) // 8 bytes per object, 8 objects = 64
        {
            og.SubObjects[n] ??= new SubObject { ObjectInfoNumber = 0xffff };
            var so = og.SubObjects[n];

            so.X = s.Int16(nameof(so.X), so.X);
            so.Z = s.Int16(nameof(so.Z), so.Z);
            so.Y = s.Int16(nameof(so.Y), so.Y);

            so.ObjectInfoNumber = s.Transform<ushort, ushort>(
                nameof(so.ObjectInfoNumber),
                so.ObjectInfoNumber,
                S.UInt16,
                StoreIncrementedConverter.Instance);

            if (so.ObjectInfoNumber == 0xffff)
                og.SubObjects[n] = null;
        } // +64

        return og;
    }
}