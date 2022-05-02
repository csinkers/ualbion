using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets;

public class CharacterSheetLoader : Component, IAssetLoader<CharacterSheet>
{
    public CharacterSheet Serdes(CharacterSheet existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        return CharacterSheet.Serdes(info.AssetId, existing, context.Mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as CharacterSheet, info, s, context);
}
