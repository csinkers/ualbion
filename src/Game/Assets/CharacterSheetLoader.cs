using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets;

public class CharacterSheetLoader : Component, IAssetLoader<CharacterSheet>
{
    public CharacterSheet Serdes(CharacterSheet existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return CharacterSheet.Serdes(info.AssetId, existing, mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes(existing as CharacterSheet, info, mapping, s, jsonUtil);
}