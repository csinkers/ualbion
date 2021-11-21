namespace UAlbion.Scripting
{
    public record LoopPart(
        int Index,
        bool Header = false,
        bool Tail = false,
        bool Break = false,
        bool Continue = false,
        bool OutsideEntry = false,
        bool Negated = false);
}