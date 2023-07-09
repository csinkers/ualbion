using UAlbion.Config;

namespace UAlbion.Formats;

public record AssetLoadResult(AssetId AssetId, object Asset, AssetNode Node);