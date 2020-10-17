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
        static readonly IDictionary<string, GameLanguage> _shortLanguageNames = new Dictionary<string, GameLanguage>
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
            var rawJson = File.ReadAllText(filename);
            var json = JsonConvert.DeserializeObject<
                Dictionary<TextId, Dictionary<string, string>>>
                (rawJson);

            _strings = json.ToDictionary(
                x => x.Key,
                x => x.Value.ToDictionary(
                    y => _shortLanguageNames[y.Key],
                    y => y.Value
                ));
        }

        public object LoadAsset(AssetId key, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
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

        public AssetInfo GetAssetInfo(AssetId key, Func<AssetId, SerializationContext, object> loaderFunc) => null;
    }
}
