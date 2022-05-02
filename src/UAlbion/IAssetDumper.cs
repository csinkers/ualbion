using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion;

public interface IAssetDumper : IComponent
{
    void Dump(string baseDir, ISet<AssetType> types, AssetId[] dumpIds);
}