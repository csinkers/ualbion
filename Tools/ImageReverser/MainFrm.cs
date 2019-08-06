using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UAlbion.Formats;
using UAlbion.Formats.Parsers;

namespace UAlbion.Tools.ImageReverser
{
    public partial class MainFrm : Form
    {
        readonly DateTime _startTime;
        readonly AssetConfig _config;
        readonly Timer _timer;
        TreeNode _rootNode;
        AlbionSprite _sprite;
        IList<int> _savedPalettes;

        public MainFrm(AssetConfig config)
        {
            _startTime = DateTime.Now;
            _config = config;
            _timer = new Timer { Interval = 250 };
            _timer.Tick += OnTimerTick;
            InitializeComponent();
        }

        void OnTimerTick(object sender, EventArgs e)
        {
            if(_sprite?.Frames.Count > 1)
            {
                var frame = trackFrame.Value;
                frame++;

                var (filename, _) = CurrentObject;
                if ((filename ?? "").Contains("MONGFX")) // Skip odd frames for monster graphics
                    frame++;

                frame = frame % _sprite.Frames.Count;
                trackFrame.Value = frame;
            }

            Render();
        }

        public event EventHandler SaveClicked;

        void MainFrm_Load(object sender, EventArgs e)
        {
            _rootNode = fileTree.Nodes.Add("Files");
            var exportedDir = Path.GetFullPath(Path.Combine(_config.BasePath, _config.ExportedXldPath));
            var files = Directory.EnumerateFiles(exportedDir, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var absDir = Path.GetDirectoryName(file);
                var relativeDir = absDir.Substring(exportedDir.Length).TrimStart('\\');
                if (relativeDir.Length == 0)
                    continue;

                if (!_config.Xlds.ContainsKey(relativeDir))
                    _config.Xlds.Add(relativeDir, new AssetConfig.Xld());

                var relative = file.Substring(exportedDir.Length + 1);
                var xld = _config.Xlds[relativeDir];
                AddToTree(relative, xld);
            }

            var commonPalette = File.ReadAllBytes(Path.Combine(_config.BasePath, _config.XldPath, "PALETTE.000"));

            var palettesPath = Path.Combine(exportedDir, "PALETTE0.XLD");
            files = Directory.EnumerateFiles(palettesPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var a = file.Substring(palettesPath.Length + 1, 2);
                int paletteNumber = int.Parse(a);
                var assetConfig = _config.Xlds["PALETTE0.XLD"].Assets[paletteNumber];
                var paletteName = assetConfig.Name;
                if (string.IsNullOrEmpty(paletteName))
                    paletteName = file.Substring(exportedDir.Length + 1);

                using(var stream = File.Open(file, FileMode.Open))
                using (var br = new BinaryReader(stream))
                {
                    var palette = new AlbionPalette(br, (int)br.BaseStream.Length, paletteName, paletteNumber);
                    palette.SetCommonPalette(commonPalette);
                    chkListPalettes.Items.Add(palette);
                }
            }

            chkListPalettes.SelectedIndex = 0;
            _rootNode.Expand();
            _timer.Start();
        }

        IAssetLoader GetLoader(AssetConfig.Asset conf)
        {
            switch (conf.Type)
            {
                case XldObjectType.AmorphousSprite: return new AmorphousSpriteLoader();
                case XldObjectType.FixedSizeSprite: return new FixedSizeSpriteLoader();

                case XldObjectType.SingleHeaderSprite:
                case XldObjectType.HeaderPerSubImageSprite: return new HeaderBasedSpriteLoader();

                default: throw new NotImplementedException();
            }
        }

        bool IsSprite(XldObjectType type)
        {
            switch (type)
            {
                case XldObjectType.AmorphousSprite: 
                case XldObjectType.FixedSizeSprite:
                case XldObjectType.SingleHeaderSprite:
                case XldObjectType.HeaderPerSubImageSprite: return true;
                default: return false;
            }
        }

        AlbionSprite LoadSprite(string filename, AssetConfig.Asset conf)
        {
            using (var stream = File.OpenRead(filename))
            using (var br = new BinaryReader(stream))
            {
                return (AlbionSprite)GetLoader(conf).Load(br, stream.Length, filename, conf);
            }
        }

        void UpdateInfo()
        {
            var (filename, asset) = CurrentObject;
            if (asset == null)
            {
                txtInfo.Text = "No asset selected";
                return;
            }

            var fileInfo = new FileInfo(filename);
            var sb = new StringBuilder();
            sb.AppendLine($"{filename}");
            sb.AppendLine($"File Size: {fileInfo.Length}");
            sb.AppendLine($"XLD: {asset.Parent.Name}");
            sb.AppendLine($"Type: {asset.Type}");
            sb.AppendLine($"Conf Width: {asset.EffectiveWidth}");
            sb.AppendLine($"Conf Height: {asset.EffectiveHeight}");

            if (_sprite != null)
            {
                sb.AppendLine($"Frame Count: {_sprite.Frames.Count}");
                sb.AppendLine($"Sprite Width: {_sprite.Width}");
                sb.AppendLine($"Sprite Height: {_sprite.Height}");

                sb.AppendLine($"Frame Width: {_sprite.Frames[trackFrame.Value].Width}");
                sb.AppendLine($"Frame Height: {_sprite.Frames[trackFrame.Value].Height}");
                sb.AppendLine($"Frame X: {_sprite.Frames[trackFrame.Value].X}");
                sb.AppendLine($"Frame Y: {_sprite.Frames[trackFrame.Value].Y}");
            }
            txtInfo.Text = sb.ToString();
        }

        Bitmap GenerateBitmap(AlbionSprite sprite, int frameNumber, int width, int magnify, uint[] palette)
        {
            var frame = sprite.Frames[frameNumber];
            var offset = frame.Y * sprite.Width;
            int height = Math.Min(frame.Height, (sprite.PixelData.Length - offset + (width - 1)) / width);
            if (height == 0)
                return new Bitmap(1, 1);
            Bitmap bmp;
            if (canvas.Image?.Width == width * magnify && canvas.Image?.Height == height * magnify)
            {
                bmp = (Bitmap)canvas.Image;
            }
            else
            {
                bmp = new Bitmap(width * magnify, height * magnify);
            }

            var d = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                for (int n = offset; n < offset + width * height && n < sprite.PixelData.Length; n++)
                {
                    unsafe
                    {
                        for (int my = 0; my < magnify; my++)
                        {
                            for (int mx = 0; mx < magnify; mx++)
                            {
                                int x = magnify * ((n - offset) % width) + mx;
                                int y = magnify * ((n - offset) / width) + my;
                                byte* p = (byte*)d.Scan0 + y * d.Stride + x * 3;
                                byte color = sprite.PixelData[n];

                                p[0] = (byte)((palette[color] & 0x0000ff00) >> 8);
                                p[1] = (byte)((palette[color] & 0x00ff0000) >> 16);
                                p[2] = (byte)((palette[color] & 0xff000000) >> 24);
                            }
                        }
                    }
                }
            }
            finally { bmp.UnlockBits(d); }

            return bmp;
        }

        /*Bitmap LoadInterlaced(string filename)
        {
            var a = new FileInfo(filename);
            return new Bitmap(filename);
        }*/

        void Render()
        {
            const int magnify = 3;
            var (filename, asset) = CurrentObject;
            if (asset == null)
                return;

            Bitmap bmp;
            /*if (asset.Type == XldObjectType.InterlacedBitmap)
            {
                _sprite = null;
                bmp = LoadInterlaced(filename);
            }
            else*/ if (IsSprite(asset.Type))
            {
                if (filename != _sprite?.Name)
                    _sprite = LoadSprite(filename, asset);

                if (_sprite == null)
                    return;

                trackFrameCount.Maximum = _sprite.Height;
                numFrameCount.Maximum = trackFrameCount.Maximum;
                trackFrame.Maximum = _sprite.Frames.Count - 1;
                numFrame.Maximum = trackFrame.Maximum;

                if (trackWidth.Value == 1)
                    trackWidth.Value = _sprite.Width;

                var palette = (AlbionPalette)(chkListPalettes.SelectedItem ?? chkListPalettes.Items[0]);
                uint[] curPalette = palette.GetPaletteAtTime((int)((DateTime.Now - _startTime).TotalSeconds * 4));

                var width = Math.Max(0, trackWidth.Value);
                var frame = Math.Max(0, trackFrame.Value);
                bmp = GenerateBitmap(_sprite, frame, width, magnify, curPalette);
            }
            else
            {
                _sprite = null;
                bmp = new Bitmap(1, 1);
            }

            canvas.Image = bmp;
            UpdateInfo();
        }

        void AddToTree(string location, AssetConfig.Xld xld)
        {
            var parts = location.Split('\\');
            TreeNode node = _rootNode;
            foreach (var dir in parts.Take(parts.Length - 1)) // Ensure parent nodes exist
            {
                if (!node.Nodes.ContainsKey(dir))
                    node.Nodes.Add(dir, dir);
                node = node.Nodes[dir];
            }

            string key = parts.Last();
            if (!key.EndsWith(".bin")) return;
            key = key.Substring(0, key.Length-4);
            int number = int.Parse(key);

            if (!xld.Assets.ContainsKey(number)) xld.Assets[number] = new AssetConfig.Asset { Width = 32 };
            AssetConfig.Asset obj = xld.Assets[number];

            string name = string.IsNullOrEmpty(obj.Name)
                ? key
                : $"{xld.Assets[number].Name} ({number})";

            if (!node.Nodes.ContainsKey(key))
                node.Nodes.Add(key, name).Tag = obj;
        }

        (string, AssetConfig.Xld) CurrentXld
        {
            get
            {
                if (fileTree.SelectedNode == null)
                    return (null, null);

                var node = fileTree.SelectedNode;
                if (int.TryParse(fileTree.SelectedNode.Name, out _))
                    node = fileTree.SelectedNode.Parent;

                string filename = "";
                while (node != _rootNode)
                {
                    filename = node.Name + "\\" + filename;
                    node = node.Parent;
                }

                filename = filename.TrimEnd('\\');
                var fullXldPath = Path.GetFullPath(Path.Combine(Path.Combine(_config.BasePath, _config.ExportedXldPath), filename));
                return _config.Xlds.ContainsKey(filename) 
                    ? (fullXldPath, _config.Xlds[filename]) 
                    : (null, null);
            }
        }

        (string, AssetConfig.Asset) CurrentObject
        {
            get
            {
                if (!int.TryParse(fileTree.SelectedNode?.Name, out int number))
                    return (null, null);

                var (xldFilename, xld) = CurrentXld;
                if (xld == null || !xld.Assets.ContainsKey(number))
                    return (null, null);

                var filename = $"{xldFilename}\\{number:00}.bin";
                return (filename, xld.Assets[number]);
            }
        }

        void SyncSelectedPalettes()
        {
            var (filename, asset) = CurrentObject;
            if (asset == null)
                return;

            if (asset.PaletteHints == null)
                asset.PaletteHints = new List<int>();

            for (int index = 0; index < chkListPalettes.Items.Count; index++)
            {
                var item = (AlbionPalette)chkListPalettes.Items[index];
                chkListPalettes.SetItemChecked(index, asset.PaletteHints.Contains(item.Id));
            }

            if (!chkListPalettes.GetItemChecked(chkListPalettes.SelectedIndex) && chkListPalettes.CheckedIndices.Count > 0)
                chkListPalettes.SelectedIndex = chkListPalettes.CheckedIndices[0];
        }

        void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var (filename, asset) = CurrentObject;
            if (asset == null)
                return;
            SyncSelectedPalettes();

            trackWidth.Value = asset.EffectiveWidth == 0 ? 1 : asset.EffectiveWidth;
            trackFrame.Value = 0;
            textName.Text = asset.Name;
            Render();

            if (_sprite != null)
            {
                trackFrameCount.Value = _sprite.Frames.Count;
                if (asset.Type == XldObjectType.FixedSizeSprite && asset.Height != null && _sprite.Frames[0].Height != asset.Height)
                    asset.Height = _sprite.Frames[0].Height;
            }
        }

        #region Width
        void TrackWidth_ValueChanged(object sender, EventArgs e)
        {
            var (filename, asset) = CurrentObject;
            if (asset == null)
                return;

            if (!asset.Parent.Width.HasValue && asset.Type == XldObjectType.FixedSizeSprite && asset.Width != trackWidth.Value)
            {
                asset.Width = trackWidth.Value;
                _sprite = null; // Force sprite reload
                Render();
            }

            if (sender != numWidth && (int)numWidth.Value != trackWidth.Value)
                numWidth.Value = trackWidth.Value;
        }

        void TrackWidth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Left && trackWidth.Value != 0)
            {
                trackWidth.Value = (int)(trackWidth.Value / (e.Shift ? 1.5 : 2.0));
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.Right)
            {
                var newValue =  (int)(trackWidth.Value * (e.Shift ? 1.5 : 2.0));
                if (newValue > trackWidth.Maximum)
                    newValue = trackWidth.Maximum;
                trackWidth.Value = newValue;
                e.Handled = true;
            }
        }

        void NumWidth_ValueChanged(object sender, EventArgs e)
        {
            if (sender != trackWidth && trackWidth.Value != (int)numWidth.Value)
                trackWidth.Value = (int)numWidth.Value;
        }

        void NumWidth_Enter(object sender, EventArgs e)
        {
            numWidth.Select(0, numWidth.Text.Length);
        }

        #endregion

        void TextName_TextChanged(object sender, EventArgs e)
        {
            var (filename, asset) = CurrentObject;
            if(asset != null)
                asset.Name = textName.Text;
        }

        void TrackFrameCount_ValueChanged(object sender, EventArgs e)
        {
            var (filename, asset) = CurrentObject;
            if (_sprite != null)
            {
                int? newHeight = trackFrameCount.Value <= 1 ? (int?)null : _sprite.Height / trackFrameCount.Value;
                if (asset != null && !asset.Parent.Height.HasValue && asset.Type == XldObjectType.FixedSizeSprite && asset.Height != newHeight)
                {
                    asset.Height = newHeight;
                    _sprite = null; // Force sprite reload
                    Render();
                }
            }

            if (sender != numFrameCount && (int)numFrameCount.Value != trackFrameCount.Value)
                numFrameCount.Value = trackFrameCount.Value;
        }

        void NumFrameCount_ValueChanged(object sender, EventArgs e)
        {
            if (sender != trackFrameCount && trackFrameCount.Value != (int)numFrameCount.Value)
                trackFrameCount.Value = (int)numFrameCount.Value;
        }

        void TrackFrame_ValueChanged(object sender, EventArgs e)
        {
            Render();
            if (sender != numFrame && (int)numFrame.Value != trackFrame.Value)
                numFrame.Value = trackFrame.Value;
        }

        void NumFrame_ValueChanged(object sender, EventArgs e)
        {
            if (sender != trackFrame && trackFrame.Value != (int)numFrame.Value)
                trackFrame.Value = (int)numFrame.Value;
        }

        void BtnSave_Click(object sender, EventArgs e)
        {
            SaveClicked?.Invoke(this, EventArgs.Empty);
        }

        void ChkListPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        void ChkAnimate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAnimate.Checked) _timer.Start();
            else _timer.Stop();
        }

        void FileTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.X && _sprite != null)
            {
                trackWidth.Value = _sprite.Frames[0].Height;
            }

            var (_, asset) = CurrentObject;
            if (e.Control && e.KeyCode == Keys.C && asset != null)
            {
                _savedPalettes = asset.PaletteHints.ToList();
            }

            if (e.Control && e.KeyCode == Keys.V && asset != null)
            {
                asset.PaletteHints.Clear();
                foreach(var palette in _savedPalettes)
                    asset.PaletteHints.Add(palette);
                SyncSelectedPalettes();
            }
        }

        void ChkListPalettes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var (_, asset) = CurrentObject;
            if (asset == null)
                return;

            var palette = (AlbionPalette) chkListPalettes.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                if (!asset.PaletteHints.Contains(palette.Id))
                    asset.PaletteHints.Add(palette.Id);
            }
            else
            {
                asset.PaletteHints.Remove(palette.Id);
            }
        }
    }
}
