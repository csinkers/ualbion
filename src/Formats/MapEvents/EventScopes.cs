namespace UAlbion.Formats.MapEvents
{
    public enum EventScopes : byte
    {
        AbsPerm = 0, // Absolute positioning, permanent lifetime
        RelPerm = 1, // Relative positioning (vs. absolute), permanent lifetime
        AbsTemp = 2, // Absolute positioning, temporary lifetime (i.e. doesn't survive map reload)
        RelTemp = 3, // Relative and temporary
    }
}
