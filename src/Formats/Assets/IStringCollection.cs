namespace UAlbion.Formats.Assets;

public interface IStringCollection
{
    int Count { get; }
    string GetString(StringId id, string language = null);
}