using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class PaletteHints
    {
        public Dictionary<string, Dictionary<string, int>> Raw { get; private set; }
        public static PaletteHints Parse(string text) 
            => new PaletteHints { Raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(text) };

        public static PaletteHints Load(string path, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(path)) throw new FileNotFoundException($"Could not open palette hint config from {path}");
            var configText = disk.ReadAllText(path);
            return Parse(configText);
        }

        public int Get(string file, int subId, int defaultValue = 0)
        {
            if (!Raw.TryGetValue(file, out var keys)) return defaultValue;
            if (keys.TryGetValue(subId.ToString(CultureInfo.InvariantCulture), out var id)) return id;
            return keys.TryGetValue("Default", out id) ? id : defaultValue;
        }
    }
}
