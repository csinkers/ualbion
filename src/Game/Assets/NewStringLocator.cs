#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets
{
    public class NewStringLocator : Component, IAssetLocator
    {
        Dictionary<TextId, Dictionary<GameLanguage, string>> _strings;
        static readonly IDictionary<string, GameLanguage> ShortLanguageNames = new Dictionary<string, GameLanguage>
        {
            { "en", GameLanguage.English },
            { "de", GameLanguage.German },
            { "fr", GameLanguage.French },
        };

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.Text };

        public NewStringLocator() => On<ReloadAssetsEvent>(_ => _strings = null);

        protected override void Subscribed() => Load();

        void Load()
        {
            if (_strings != null)
                return;

            var settings = Resolve<ISettings>();
            var filename = Path.Combine(settings.BasePath, "data", "strings.json");
            var rawJson = disk.ReadAllText(filename);
            var json = JsonConvert.DeserializeObject<
                Dictionary<string, Dictionary<string, string>>>
                (rawJson);

            _strings = json.ToDictionary(
                x =>
                    Enum.TryParse(x.Key, true, out Base.UAlbionString id)
                        ? TextId.From(id)
                        : throw new InvalidOperationException(
                            $"When loading UAlbion strings, the key {x.Key} did not match any Base.UAlbionString entry."),
                x => x.Value.ToDictionary(
                    y => ShortLanguageNames[y.Key],
                    y => y.Value
                ));
        }

        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            Load();
            if (!_strings.TryGetValue((TextId)key.Id, out var languages))
            {
                Raise(new LogEvent(LogEvent.Level.Error,
                    $"No strings found for {(TextId)key.Id}"));
                return $"MISSING!{(TextId)key.Id}";
            }

            if (languages.TryGetValue(context.Language, out var s))
                return s;

            Raise(new LogEvent(LogEvent.Level.Warning,
                $"Missing translation for {(TextId)key.Id} in {context.Language}"));
            return languages[GameLanguage.English]; // Default
        }
    }
}
#endif
