namespace UAlbion.Formats.Assets;

public interface IStringSet
{
    int Count { get; }
    string GetString(StringId id);
    void SetString(StringId id, string value);
}