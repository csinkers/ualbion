using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UAlbion.Config;

namespace UAlbion.Formats;

public record AssetConversionOptions(
    AssetLoaderMethod LoaderFunc,
    Action FlushCacheFunc,
    ISet<AssetId> Ids,
    ISet<AssetType> AssetTypes,
    string[] Languages,
    Regex FilePattern
);