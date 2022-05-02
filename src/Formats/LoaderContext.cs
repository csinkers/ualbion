using System;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats;

public class LoaderContext
{
    public LoaderContext(IAssetManager assets, IJsonUtil json, AssetMapping mapping)
    {
        Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        Json = json ?? throw new ArgumentNullException(nameof(json));
        Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    public IAssetManager Assets { get; }
    public IJsonUtil Json { get; }
    public AssetMapping Mapping { get; }
}