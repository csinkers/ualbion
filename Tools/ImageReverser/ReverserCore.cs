using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Tools.ImageReverser
{
    public class ReverserCore
    {
        static readonly string[] SupportedExtensions = { "bin", "wav", "json", "xmi" };
        public string BaseExportDirectory { get; }
        public FullAssetConfig Config { get; }
        public GeneralConfig GeneralConfig { get; }
        public IDictionary<string, FullXldInfo> Xlds { get; } = new Dictionary<string, FullXldInfo>();
        public IList<AlbionPalette> Palettes { get; } = new List<AlbionPalette>();

        public ReverserCore(GeneralConfig generalConfig, FullAssetConfig config)
        {
            GeneralConfig = generalConfig;
            Config = config;

            BaseExportDirectory = Path.GetFullPath(Path.Combine(GeneralConfig.BasePath, GeneralConfig.ExportedXldPath));
            var files = Directory.EnumerateFiles(BaseExportDirectory, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var absDir = Path.GetDirectoryName(file) ?? "";
                var relativeDir = absDir.Substring(BaseExportDirectory.Length).TrimStart('\\').Replace("\\", "/");
                if (relativeDir.Length == 0)
                    continue;

                if (!Config.Xlds.ContainsKey(relativeDir))
                    Config.Xlds.Add(relativeDir, new FullXldInfo());

                var xld = Config.Xlds[relativeDir];
                if(!Xlds.ContainsKey(relativeDir))
                    Xlds.Add(relativeDir, xld);

                var relative = file.Substring(BaseExportDirectory.Length + 1);
                var parts = relative.Split('\\');
                string key = parts.Last();
                key = Path.GetFileNameWithoutExtension(key);
                if (!int.TryParse(key, out var number))
                    continue;

                if (!xld.Assets.ContainsKey(number))
                    xld.Assets[number] = new FullAssetInfo();

                FullAssetInfo asset = xld.Assets[number];
                asset.Filename = relative;
            }

            var commonPalette = File.ReadAllBytes(Path.Combine(GeneralConfig.BasePath, GeneralConfig.XldPath, "PALETTE.000"));

            var palettesPath = Path.Combine(BaseExportDirectory, "PALETTE0.XLD");
            files = Directory.EnumerateFiles(palettesPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var a = file.Substring(palettesPath.Length + 1, 2);
                int paletteNumber = int.Parse(a);
                var assetConfig = Config.Xlds["PALETTE0.XLD"].Assets[paletteNumber];
                var paletteName = assetConfig.Name;
                if (string.IsNullOrEmpty(paletteName))
                    paletteName = file.Substring(BaseExportDirectory.Length + 1);

                using(var stream = File.Open(file, FileMode.Open))
                using (var br = new BinaryReader(stream))
                {
                    var palette = new AlbionPalette(br, (int)br.BaseStream.Length, paletteName, paletteNumber);
                    palette.SetCommonPalette(commonPalette);
                    Palettes.Add(palette);
                }
            }
        }

        string _selectedXldPath;
        int? _selectedItemNumber;
        public void SetSelectedItem(string xld, int? item)
        {
            _selectedXldPath = xld;
            _selectedItemNumber = item;
            SelectionChanged?.Invoke(this, new SelectedAssetChangedArgs(SelectedXld, SelectedObject));
        }
        public void TriggerAssetChanged(FullAssetInfo asset) => AssetChanged?.Invoke(this, new AssetChangedArgs(asset));
        public event EventHandler<SelectedAssetChangedArgs> SelectionChanged;
        public event EventHandler<AssetChangedArgs> AssetChanged;


        public FullXldInfo SelectedXld =>
            _selectedXldPath != null
            ? Xlds.TryGetValue(_selectedXldPath, out var xld)
                ? xld
                : null
            : null;

        public FullAssetInfo SelectedObject =>
            SelectedXld != null && _selectedItemNumber.HasValue
                ? SelectedXld.Assets.TryGetValue(_selectedItemNumber.Value, out var asset)
                    ? asset
                    : null
                : null;
    }

    public class AssetChangedArgs
    {
        public AssetChangedArgs(FullAssetInfo asset)
        {
            Asset = asset;
        }

        public FullAssetInfo Asset { get; }
    }

    public class SelectedAssetChangedArgs
    {
        public SelectedAssetChangedArgs(FullXldInfo selectedXld, FullAssetInfo selectedObject)
        {
            SelectedXld = selectedXld;
            SelectedObject = selectedObject;
        }

        public FullXldInfo SelectedXld { get; }
        public FullAssetInfo SelectedObject { get; }
    }
}
