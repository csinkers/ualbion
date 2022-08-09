using ADLMidi.NET;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public class SoundBankLoader : Component, IAssetLoader
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => GlobalTimbreLibrary.Serdes((GlobalTimbreLibrary)existing, s);
}