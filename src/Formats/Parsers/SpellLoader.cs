using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class SpellLoader : IAssetLoader<SpellData>
{
    public static readonly AssetIdAssetProperty          SpellName   = new("Name"); // StringId to use in game
    public static readonly EnumAssetProperty<SpellClass> MagicSchool = new("School", SpellClass.DjiKas); // SpellClass enum
    public static readonly IntAssetProperty              SpellNumber = new("SpellNumber"); // offset into school, used for save-game serialization

    public SpellData Serdes(SpellData existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));
        return SpellData.Serdes(existing, context, s);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as SpellData, s, context);
}
