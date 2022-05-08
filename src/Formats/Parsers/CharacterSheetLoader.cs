using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class CharacterSheetLoader : Component, IAssetLoader<CharacterSheet>
{
    public CharacterSheet Serdes(CharacterSheet existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        return CharacterSheet.Serdes(info.AssetId, existing, context.Mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes(existing as CharacterSheet, info, s, context);
}