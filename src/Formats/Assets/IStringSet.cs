namespace UAlbion.Formats.Assets;

public interface IStringSet
{
    int Count { get; }
    string GetString(StringId id, string language = null);
}