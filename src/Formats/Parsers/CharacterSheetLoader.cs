using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Sheets;

namespace UAlbion.Formats.Parsers;

public class CharacterSheetLoader : Component, IAssetLoader<CharacterSheet>
{
    public CharacterSheet Serdes(CharacterSheet existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CharacterSheet.Serdes(context.AssetId, existing, context.Mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as CharacterSheet, s, context);
}