using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UAlbion.Config;

namespace UAlbion.Tools.ImageReverser
{
    public partial class MainFrm : Form
    {
        readonly IDictionary<AssetInfo, TreeNode> _nodes = new Dictionary<AssetInfo, TreeNode>();
        readonly ReverserCore _core;
        readonly Font _boldFont = new Font(DefaultFont, FontStyle.Bold);
        readonly Font _defaultFont = new Font(DefaultFont, 0);
        readonly ImageViewer _imageViewer;
        readonly TextViewer _textViewer;
        readonly SoundViewer _soundPlayer;
        int? _savedPalette;
        IAssetViewer _activeViewer;
        TreeNode _rootNode;

        public MainFrm(ReverserCore core)
        {
            _core = core;
            _core.AssetChanged += CoreOnAssetChanged;
            _imageViewer = new ImageViewer(_core) { Visible = false };
            _textViewer = new TextViewer { Visible = false };
            _soundPlayer = new SoundViewer(_core) { Visible = false };
            InitializeComponent();
            mainPanel.Controls.Add(_imageViewer);
            mainPanel.Controls.Add(_textViewer);
            mainPanel.Controls.Add(_soundPlayer);
            ResumeLayout(false);
            PerformLayout();
        }

        void CoreOnAssetChanged(object sender, AssetChangedArgs e)
        {
            _nodes[e.Asset].NodeFont = e.Asset.PaletteHint == null ? _boldFont : _defaultFont;
        }

        public event EventHandler SaveClicked;

        void MainFrm_Load(object sender, EventArgs e)
        {
            _rootNode = fileTree.Nodes.Add("Files");
            foreach(var xld in _core.ContainerFiles)
                foreach (var asset in xld.Value.Assets.Values)
                    AddToTree(asset, _core.GetRawPath(asset));
            _rootNode.Expand();

            // this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FileTree_AfterSelect);
        }

        void UpdateAssetDescription()
        {
            var asset = _core.SelectedObject;
            if (asset == null)
            {
                txtInfo.Text = "No asset selected";
                return;
            }

            var filename = _core.GetRawPath(asset);

            var sb = new StringBuilder();
            sb.AppendLine($"{filename}");

            if (File.Exists(filename))
            {
                var fileInfo = new FileInfo(filename);
                sb.AppendLine($"File Size: {fileInfo.Length}");
                sb.AppendLine($"Path: {asset.File.Filename}");
                sb.AppendLine($"Layer: {asset.File.Format}");
                sb.AppendLine($"Conf Width: {asset.EffectiveWidth}");
                sb.AppendLine($"Conf Height: {asset.EffectiveHeight}");
            }

            sb.AppendLine();
            _activeViewer?.GetAssetDescription(sb);

            txtInfo.Text = sb.ToString();
        }

        void AddToTree(AssetInfo asset, string filename)
        {
            if (filename == null || !filename.StartsWith(_core.BaseExportDirectory))
                return;

            filename = filename.Substring(_core.BaseExportDirectory.Length + 1);

            var parts = filename.Split('\\');
            TreeNode node = _rootNode;
            foreach (var dir in parts.Take(parts.Length - 1)) // Ensure parent nodes exist
            {
                var noExtension = Path.GetFileNameWithoutExtension(dir);
                if (!node.Nodes.ContainsKey(noExtension))
                    node.Nodes.Add(noExtension, noExtension);
                node = node.Nodes[noExtension];
            }

            string key = Path.GetFileNameWithoutExtension(parts.Last());
            string name = string.IsNullOrEmpty(asset.Name)
                ? key
                : $"{asset.Name} ({asset.Id})";

            if (!node.Nodes.ContainsKey(key))
            {
                var newNode = node.Nodes.Add(key, name);
                newNode.Tag = asset;
                newNode.NodeFont = asset.PaletteHint == null ? _boldFont : _defaultFont;
                _nodes[asset] = newNode;
            }
        }

        void BtnSave_Click(object sender, EventArgs e)
        {
            SaveClicked?.Invoke(this, EventArgs.Empty);
        }

        void FileTree_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.Control && e.KeyCode == Keys.X && _logicalSprite != null)
            {
                trackWidth.Value = _logicalSprite.Frames[0].Height;
            } */

            var asset = _core.SelectedObject;
            if (e.Control && e.KeyCode == Keys.C && asset != null)
                _savedPalette = asset.PaletteHint;

            if (e.Control && e.KeyCode == Keys.V && asset != null && _savedPalette != null)
                asset.PaletteHint = _savedPalette;
        }

        void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = fileTree.SelectedNode;
            if (node == null)
            {
                _core.SetSelectedItem(null, null);
                return;
            }

            int? number = int.TryParse(fileTree.SelectedNode?.Name, out var tempNumber) ? tempNumber : (int?)null;

            if (number.HasValue)
                node = node.Parent;

            string filename = "";
            while (node != _rootNode)
            {
                filename = node.Name + "\\" + filename;
                node = node.Parent;
            }

            filename = filename.TrimEnd('\\');
            _core.SetSelectedItem(filename, number);
            var asset = _core.SelectedObject;
            textName.Text = asset?.Name;

            _activeViewer = GetViewerForAsset(asset);
            _imageViewer.Visible = _activeViewer == _imageViewer;
            _textViewer.Visible = _activeViewer == _textViewer;
            _soundPlayer.Visible = _activeViewer == _soundPlayer;

            UpdateAssetDescription();
        }

        IAssetViewer GetViewerForAsset(AssetInfo asset)
        {
            if (asset == null)
                return null;

            switch (asset.File.Loader)
            {
                case "UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.FontSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.HeaderBasedSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.InterlacedBitmapLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.SlabLoader, UAlbion.Formats":
                    return _imageViewer;

                case "UAlbion.Formats.Parsers.AlbionStringTableLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.SystemTextLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.ItemNameLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.ScriptLoader, UAlbion.Formats":
                    return _textViewer;

                case "UAlbion.Formats.Parsers.SampleLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.SongLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.WaveLibLoader, UAlbion.Formats":
                    return _soundPlayer;
            }

            return null;
        }

        void TextName_TextChanged(object sender, EventArgs e)
        {
            var asset = _core.SelectedObject;
            if(asset != null)
                asset.Name = textName.Text;
        }
    }
}
