using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets;

public class SavedGameLoader : Component, IAssetLoader<SavedGame>
{
    public SavedGame Serdes(SavedGame existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return SavedGame.Serdes(existing, context.Mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as SavedGame, s, context);
}
