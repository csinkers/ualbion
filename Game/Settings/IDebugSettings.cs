namespace UAlbion.Game.Settings
{
    public interface IDebugSettings
    {
        bool DrawPositions { get; }
        bool HighlightTile { get; }
        bool HighlightSelection { get; }
        bool HighlightEventChainZones { get; }
        bool ShowPaths { get; }
    }
}