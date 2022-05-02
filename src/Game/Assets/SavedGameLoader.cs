using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets;

public class SavedGameLoader : Component, IAssetLoader<SavedGame>
{
    public SavedGame Serdes(SavedGame existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return SavedGame.Serdes(existing, context.Mapping, s, Resolve<ISpellManager>());
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as SavedGame, info, s, context);
}
