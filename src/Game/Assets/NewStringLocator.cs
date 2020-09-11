﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets
{
    public class NewStringLocator : Component, IAssetLocator
    {
        Dictionary<UAlbionStringId, Dictionary<GameLanguage, string>> _strings;
        static readonly IDictionary<string, GameLanguage> _shortLanguageNames = new Dictionary<string, GameLanguage>
        {
            { "en", GameLanguage.English },
            { "de", GameLanguage.German },
            { "fr", GameLanguage.French },
        };

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.UAlbionText };

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
                Dictionary<UAlbionStringId, Dictionary<string, string>>>
                (rawJson);

            _strings = json.ToDictionary(
                x => x.Key,
                x => x.Value.ToDictionary(
                    y => _shortLanguageNames[y.Key],
                    y => y.Value
                ));
        }

        public object LoadAsset(AssetKey key, string name, Func<AssetKey, object> loaderFunc)
        {
            Load();
            if (!_strings.TryGetValue((UAlbionStringId)key.Id, out var languages))
            {
                Raise(new LogEvent(LogEvent.Level.Error,
                    $"No strings found for {(UAlbionStringId)key.Id}"));
                return $"MISSING!{(UAlbionStringId)key.Id}";
            }

            if (languages.TryGetValue(key.Language, out var s))
                return s;

            Raise(new LogEvent(LogEvent.Level.Warning,
                $"Missing translation for {(UAlbionStringId)key.Id} in {key.Language}"));
            return languages[GameLanguage.English]; // Default
        }

        public AssetInfo GetAssetInfo(AssetKey key, Func<AssetKey, object> loaderFunc) => null;
    }
}
