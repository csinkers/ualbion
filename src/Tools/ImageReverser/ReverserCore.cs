using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

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

        public ReverserCore(GeneralConfig generalConfig, AssetConfig config)
        {
            GeneralConfig = generalConfig;
            Config = config;

            BaseExportDirectory = Path.GetFullPath(Path.Combine(GeneralConfig.BasePath, GeneralConfig.ExportedXldPath));
            var files = Directory.EnumerateFiles(BaseExportDirectory, "*.*", SearchOption.AllDirectories);

            var fileLookup =
                (from t in config.Types
                from f in t.Value.Files
                select (f.Value.Name, f.Value))
                .ToDictionary(x => x.Name, x => x.Value);

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
                if(!ContainerFiles.ContainsKey(relativeDir))
                    ContainerFiles.Add(relativeDir, fileInfo);

                var relative = file.Substring(BaseExportDirectory.Length + 1);
                var parts = relative.Split('\\');
                string key = parts.Last();
                key = Path.GetFileNameWithoutExtension(key);
                if (!int.TryParse(key, out var number))
                    continue;

                if (!fileInfo.Assets.ContainsKey(number))
                    fileInfo.Assets[number] = new AssetInfo();

                AssetInfo asset = fileInfo.Assets[number];
                asset.Filename = relative;
            }

            var commonPalette = File.ReadAllBytes(Path.Combine(GeneralConfig.BasePath, GeneralConfig.XldPath, "PALETTE.000"));

            var palettesPath = Path.Combine(BaseExportDirectory, "PALETTE0.XLD");
            files = Directory.EnumerateFiles(palettesPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var a = file.Substring(palettesPath.Length + 1, 2);
                ushort paletteNumber = ushort.Parse(a);
                var assetConfig = fileLookup["PALETTE0"].Assets[paletteNumber];
                var paletteName = assetConfig.Name;
                if (string.IsNullOrEmpty(paletteName))
                    paletteName = file.Substring(BaseExportDirectory.Length + 1);

                using var stream = File.Open(file, FileMode.Open);
                using var br = new BinaryReader(stream);
                var palette = new AlbionPalette(br, (int)br.BaseStream.Length, new PaletteId(AssetType.Palette, paletteNumber));
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
                ? SelectedAssetFile.Assets.TryGetValue(_selectedItemNumber.Value, out var asset)
                    ? asset
                    : null
                : null;
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
