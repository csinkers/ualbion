using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

public class DummyContainer : IAssetContainer
{
    static readonly EmptySerdes Empty = new();
    public ISerdes Read(string path, AssetLoadContext context) => Empty;
    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context) { }
}