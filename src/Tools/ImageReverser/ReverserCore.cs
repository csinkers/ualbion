using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Tools.ImageReverser
{
    public class ReverserCore
    {
        // static readonly string[] SupportedExtensions = { "bin", "wav", "json", "xmi" };
        public string BaseExportDirectory { get; }
        public AssetConfig Config { get; }
        public GeneralConfig GeneralConfig { get; }
        public IDictionary<string, AssetFileInfo> ContainerFiles { get; } = new Dictionary<string, AssetFileInfo>();
        public IList<AlbionPalette> Palettes { get; } = new List<AlbionPalette>();
        readonly IDictionary<AssetInfo, string> _rawFilePaths = new Dictionary<AssetInfo, string>();
        readonly IDictionary<string, string> _extraPaths = new Dictionary<string, string> { { "LANG", "ENGLISH" } };

        static AlbionPalette LoadPalette(string file, AssetInfo assetInfo)
        {
            using var stream = File.Open(file, FileMode.Open);
            using var br = new BinaryReader(stream);
            using var s = new AlbionReader(br, stream.Length);
            return AlbionPalette.Serdes(null, assetInfo, s);
        }

        public ReverserCore(GeneralConfig generalConfig, AssetConfig config)
        {
            GeneralConfig = generalConfig;
            Config = config;

            BaseExportDirectory = Path.GetFullPath(GeneralConfig.ResolvePath("$(EXPORT)/raw", _extraPaths));
            var files = Directory.EnumerateFiles(BaseExportDirectory, "*.*", SearchOption.AllDirectories);

            var fileLookup = config.Files
                .Select(f => (f.Value.Filename, f.Value))
                .ToDictionary(x => x.Filename, x => x.Value);

            foreach (var file in files)
            {
                var absDir = Path.GetDirectoryName(file) ?? "";
                var relativeDir = absDir.Substring(BaseExportDirectory.Length).TrimStart('\\').Replace("\\", "/");
                relativeDir = Path.GetFileNameWithoutExtension(relativeDir);
                if (relativeDir.Length == 0)
                    continue;

                if (!fileLookup.TryGetValue(relativeDir, out var fileInfo))
                    continue;

                //if (!Config.Files.ContainsKey(relativeDir))
                //    Config.Files.Add(relativeDir, new AssetFileInfo());
                // var xld = Config.Files[relativeDir];
                if (!ContainerFiles.ContainsKey(relativeDir))
                    ContainerFiles.Add(relativeDir, fileInfo);

                var relative = file.Substring(BaseExportDirectory.Length + 1);
                var parts = relative.Split('\\');
                string key = parts.Last();
                key = Path.GetFileNameWithoutExtension(key);
                if (!int.TryParse(key, out var number))
                    continue;

                if (!fileInfo.Map.ContainsKey(number))
                {
                    var info = new AssetInfo { File = fileInfo, };
                    info.Set("Offset", number);
                    fileInfo.Map[number] = info;
                }

                AssetInfo asset = fileInfo.Map[number];
                _rawFilePaths[asset] = file;
            }

            var commonPalette = LoadPalette(GeneralConfig.ResolvePath("$(ALBION)/CD/XLDLIBS/PALETTE.000", _extraPaths), new AssetInfo { Name = "Common" });

            var palettesPath = Path.Combine(BaseExportDirectory, "PALETTE0.XLD");
            files = Directory.EnumerateFiles(palettesPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var a = file.Substring(palettesPath.Length + 1, 2);
                ushort paletteNumber = ushort.Parse(a);
                var assetInfo = fileLookup["PALETTE0"].Map[paletteNumber];
                var palette = LoadPalette(file, assetInfo);
                palette.SetCommonPalette(commonPalette);
                Palettes.Add(palette);
            }
        }

        string _selectedXldPath;
        int? _selectedItemNumber;
        public void SetSelectedItem(string xld, int? item)
        {
            _selectedXldPath = xld;
            _selectedItemNumber = item;
            SelectionChanged?.Invoke(this, new SelectedAssetChangedArgs(SelectedAssetFile, SelectedObject));
        }
        public void TriggerAssetChanged(AssetInfo asset) => AssetChanged?.Invoke(this, new AssetChangedArgs(asset));
        public event EventHandler<SelectedAssetChangedArgs> SelectionChanged;
        public event EventHandler<AssetChangedArgs> AssetChanged;

        public AssetFileInfo SelectedAssetFile =>
            _selectedXldPath != null
            ? ContainerFiles.TryGetValue(_selectedXldPath, out var xld)
                ? xld
                : null
            : null;

        public AssetInfo SelectedObject =>
            SelectedAssetFile != null && _selectedItemNumber.HasValue
                ? SelectedAssetFile.Map.TryGetValue(_selectedItemNumber.Value, out var asset)
                    ? asset
                    : null
                : null;

        public string GetRawPath(AssetInfo asset) => _rawFilePaths.TryGetValue(asset, out var filename) ? filename : null;
    }

    public class AssetChangedArgs
    {
        public AssetChangedArgs(AssetInfo asset)
        {
            Asset = asset;
        }

        public AssetInfo Asset { get; }
    }

    public class SelectedAssetChangedArgs
    {
        public SelectedAssetChangedArgs(AssetFileInfo selectedAssetFile, AssetInfo selectedObject)
        {
            SelectedAssetFile = selectedAssetFile;
            SelectedObject = selectedObject;
        }

        public AssetFileInfo SelectedAssetFile { get; }
        public AssetInfo SelectedObject { get; }
    }
}
