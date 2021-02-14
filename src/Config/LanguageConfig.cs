using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class LanguageConfig
    {
        [JsonIgnore] public string Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}