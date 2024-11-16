using ADLMidi.NET;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public class SoundBankLoader : Component, IAssetLoader
{
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => GlobalTimbreLibrary.Serdes((GlobalTimbreLibrary)existing, s);
}