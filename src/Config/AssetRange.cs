namespace UAlbion.Config;

public readonly record struct AssetRange(AssetId From, AssetId To)
{
    public override string ToString() => $"{From}-{To} [{From.Id}-{To.Id}]";
};