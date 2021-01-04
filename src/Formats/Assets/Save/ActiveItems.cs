using System;

namespace UAlbion.Formats.Assets.Save
{
    [Flags]
    public enum ActiveItems : uint
    {
        Compass = 1,
        MonsterEye = 2,
        Clock = 8
    }
}