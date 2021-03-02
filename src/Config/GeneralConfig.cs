using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class GeneralConfig : IGeneralConfig
    {
        static readonly Regex Pattern = new Regex(@"(\$\([A-Z]+\))");
        [JsonIgnore] public string BasePath { get; set; }
        public IDictionary<string, string> Paths { get; } = new Dictionary<string, string>();

        public static GeneralConfig Load(string configPath, string baseDir)
        {
            var config = File.Exists(configPath) 
                ? JsonConvert.DeserializeObject<GeneralConfig>(File.ReadAllText(configPath)) 
                : new GeneralConfig();

            config.BasePath = baseDir;
            return config;
        }

        public void SetPath(string pathName, string path) => Paths[pathName] = path;

        public string ResolvePath(string relative, IDictionary<string, string> extraPaths = null)
        {
            if (string.IsNullOrEmpty(relative))
                throw new ArgumentNullException(nameof(relative));

            if (relative.Contains(".."))
                throw new ArgumentOutOfRangeException($"Paths containing .. are not allowed ({relative})");

            if (relative.Contains(":") && !relative.StartsWith(BasePath, StringComparison.InvariantCulture))
                throw new ArgumentOutOfRangeException($"Paths containing : are not allowed ({relative})");

            var resolved = Pattern.Replace(relative, x =>
            {
                var name = x.Groups[0].Value.Substring(2).TrimEnd(')').ToUpperInvariant();
                if (extraPaths != null && extraPaths.TryGetValue(name, out var value))
                    return value;

                if (Paths.TryGetValue(name, out value))
                    return value;

                throw new InvalidOperationException($"Could not find path substitution for {name} in path {relative}");
            });

            return Path.Combine(BasePath, resolved);
        }
    }
}
