using UAlbion.Config;

namespace UAlbion.Base;

public enum FontGfx : byte
{
    Regular = 1,
    Bold = 2,
    Debug = 3,
    [OptionalAsset] GermanRegular = 4,
    [OptionalAsset] GermanBold = 5
}