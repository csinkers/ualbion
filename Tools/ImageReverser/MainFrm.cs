using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UAlbion.Formats.Config;

namespace UAlbion.Tools.ImageReverser
{
    public partial class MainFrm : Form
    {
        readonly IDictionary<FullAssetInfo, TreeNode> _nodes = new Dictionary<FullAssetInfo, TreeNode>();
        readonly ReverserCore _core;
        readonly Font _boldFont = new Font(DefaultFont, FontStyle.Bold);
        readonly Font _defaultFont = new Font(DefaultFont, 0);
        readonly ImageViewer _imageViewer;
        readonly TextViewer _textViewer;
        readonly SoundViewer _soundPlayer;
        IList<int> _savedPalettes;
        IAssetViewer _activeViewer;
        TreeNode _rootNode;

        public MainFrm(ReverserCore core)
        {
            _core = core;
            _core.AssetChanged += CoreOnAssetChanged;
            _imageViewer = new ImageViewer(_core) { Visible = false };
            _textViewer = new TextViewer(_core) { Visible = false };
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
            _nodes[e.Asset].NodeFont = e.Asset.PaletteHints?.Count == 0 ? _boldFont : _defaultFont;
        }

        public event EventHandler SaveClicked;

        void MainFrm_Load(object sender, EventArgs e)
        {
            _rootNode = fileTree.Nodes.Add("Files");
            foreach(var xld in _core.Xlds)
                foreach (var asset in xld.Value.Assets.Values)
                    AddToTree(asset);
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

            var sb = new StringBuilder();
            sb.AppendLine($"{asset.Filename}");

            if (File.Exists(asset.Filename))
            {
                var fileInfo = new FileInfo(asset.Filename);
                sb.AppendLine($"File Size: {fileInfo.Length}");
                sb.AppendLine($"XLD: {asset.Parent.Name}");
                sb.AppendLine($"Layer: {asset.Parent.Format}");
                sb.AppendLine($"Conf Width: {asset.EffectiveWidth}");
                sb.AppendLine($"Conf Height: {asset.EffectiveHeight}");
            }

            sb.AppendLine();
            _activeViewer?.GetAssetDescription(sb);

            txtInfo.Text = sb.ToString();
        }

        void AddToTree(FullAssetInfo asset)
        {
            if (asset.Filename == null)
                return;
            var parts = asset.Filename.Split('\\');
            TreeNode node = _rootNode;
            foreach (var dir in parts.Take(parts.Length - 1)) // Ensure parent nodes exist
            {
                if (!node.Nodes.ContainsKey(dir))
                    node.Nodes.Add(dir, dir);
                node = node.Nodes[dir];
            }

            string key = Path.GetFileNameWithoutExtension(parts.Last());
            string name = string.IsNullOrEmpty(asset.Name)
                ? key
                : $"{asset.Name} ({asset.Id})";

            if (!node.Nodes.ContainsKey(key))
            {
                var newNode = node.Nodes.Add(key, name);
                newNode.Tag = asset;
                newNode.NodeFont = asset.PaletteHints?.Count == 0 ? _boldFont : _defaultFont;
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
            {
                _savedPalettes = asset.PaletteHints.ToList();
            }

            if (e.Control && e.KeyCode == Keys.V && asset != null && _savedPalettes != null)
            {
                asset.PaletteHints.Clear();
                foreach(var palette in _savedPalettes)
                    asset.PaletteHints.Add(palette);
            }
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

        IAssetViewer GetViewerForAsset(FullAssetInfo asset)
        {
            if (asset == null)
                return null;

            switch (asset.Format)
            {
                case FileFormat.Unknown:
                    break;

                case FileFormat.AmorphousSprite:
                case FileFormat.FixedSizeSprite:
                case FileFormat.HeaderPerSubImageSprite:
                case FileFormat.SingleHeaderSprite:
                case FileFormat.Font:
                case FileFormat.InterlacedBitmap:
                case FileFormat.Slab:
                    return _imageViewer;

                case FileFormat.StringTable:
                case FileFormat.SystemText:
                case FileFormat.Script:
                case FileFormat.ItemNames:
                    return _textViewer;

                case FileFormat.AudioSample:
                case FileFormat.SampleLibrary:
                case FileFormat.Song:
                    return _soundPlayer;

                case FileFormat.Palette:
                case FileFormat.PaletteCommon:
                    break;

                case FileFormat.Video:
                    break;

                case FileFormat.MapData:
                case FileFormat.IconData:
                case FileFormat.CharacterData:
                case FileFormat.SpellData:
                case FileFormat.LabyrinthData:
                case FileFormat.ItemData:
                    break;

                case FileFormat.BlockList:
                case FileFormat.EventSet:
                case FileFormat.Inventory:
                case FileFormat.MonsterGroup:
                case FileFormat.TranslationTable:
                    break;
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
