using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class CharacterSheetLoader : Component, IAssetLoader<CharacterSheet>
{
    public CharacterSheet Serdes(CharacterSheet existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CharacterSheet.Serdes(context.AssetId, existing, context.Mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as CharacterSheet, s, context);
}