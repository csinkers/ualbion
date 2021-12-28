using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class LanguageConfig
{
    [JsonIgnore] public string Id { get; set; }
    public string DisplayName { get; set; }
    public string ShortName { get; set; }
}