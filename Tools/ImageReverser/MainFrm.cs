using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UAlbion.Formats;

namespace UAlbion.Tools.ImageReverser
{
    public partial class MainFrm : Form
    {
        readonly DateTime _startTime;
        readonly Config _config;
        TreeNode _rootNode;

        public MainFrm(Config config)
        {
            _startTime = DateTime.Now;
            _config = config;
            InitializeComponent();
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
                    _config.Xlds.Add(relativeDir, new Config.Xld());

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
                var paletteName = _config.Xlds["PALETTE0.XLD"].Objects[paletteNumber].Name;
                if (string.IsNullOrEmpty(paletteName))
                    paletteName = file.Substring(exportedDir.Length + 1);

                using(var stream = File.Open(file, FileMode.Open))
                using (var br = new BinaryReader(stream))
                {
                    var context = new AlbionPalette.PaletteContext(paletteNumber, paletteName, commonPalette);
                    var palette = new AlbionPalette(br, (int) br.BaseStream.Length, context);
                    listPalettes.Items.Add(palette);
                }
            }

            listPalettes.SelectedIndex = 0;

            _rootNode.Expand();
        }

        Bitmap LoadRawTexture(string filename, Config.Texture texture, int magnify)
        {
            var bytes = File.ReadAllBytes(filename);

            if (texture.Offset > bytes.Length)
                texture.Offset = bytes.Length;

            trackOffset.Maximum = bytes.Length - 1;
            numOffset.Maximum = trackOffset.Maximum;
            trackOffset.LargeChange = texture.Width * 400;

            int height = (bytes.Length - texture.Offset + (texture.Width - 1)) / texture.Width;
            if (height == 0)
                return new Bitmap(1, 1);

            Bitmap bmp = new Bitmap(texture.Width * magnify, height * magnify);

            var d = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            var palette = (AlbionPalette)(listPalettes.SelectedItem ?? listPalettes.Items[0]);
            var curPalette = palette.GetPaletteAtTime((int) (DateTime.Now - _startTime).TotalSeconds * 4);

            try
            {
                for (int n = texture.Offset; n < bytes.Length; n++)
                {
                    unsafe
                    {
                        for (int my = 0; my < magnify; my++)
                        {
                            for (int mx = 0; mx < magnify; mx++)
                            {
                                int x = magnify * ((n - texture.Offset) % texture.Width) + mx;
                                int y = magnify * ((n - texture.Offset) / texture.Width) + my;
                                byte* p = (byte*)d.Scan0 + y * d.Stride + x * 3;
                                byte color = bytes[n];

                                p[0] = (byte)((curPalette[color] & 0x0000ff00) >> 8);
                                p[1] = (byte)((curPalette[color] & 0x00ff0000) >> 16);
                                p[2] = (byte)((curPalette[color] & 0xff000000) >> 24);
                            }
                        }
                    }
                }
            }
            finally { bmp.UnlockBits(d); }

            return bmp;
        }

        Bitmap LoadInterlaced(string filename)
        {
            var a = new FileInfo(filename);
            return new Bitmap(filename);
        }

        void Render()
        {
            const int magnify = 3;
            var (filename, obj) = CurrentObject;
            if (obj is Config.Texture texture)
            {
                var bmp = texture.Type == "texture"
                        ? LoadRawTexture(filename, texture, magnify)
                        : LoadInterlaced(filename);

                canvas.Image = bmp;
            }
        }

        void AddToTree(string location, Config.Xld xld)
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

            if (!xld.Objects.ContainsKey(number)) xld.Objects[number] = new Config.Texture { Width = 32 };
            Config.ConfigObject obj = xld.Objects[number];

            string name = string.IsNullOrEmpty(obj.Name)
                ? key
                : $"{xld.Objects[number].Name} ({number})";

            if (!node.Nodes.ContainsKey(key))
                node.Nodes.Add(key, name).Tag = obj;
        }

        (string, Config.Xld) CurrentXld
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

        (string, Config.ConfigObject) CurrentObject
        {
            get
            {
                if (!int.TryParse(fileTree.SelectedNode?.Name, out int number))
                    return (null, null);

                var (xldFilename, xld) = CurrentXld;
                if (xld == null || !xld.Objects.ContainsKey(number))
                    return (null, null);

                var filename = $"{xldFilename}\\{number:00}.bin";
                return (filename, xld.Objects[number]);
            }
        }

        void FileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var (filename, obj) = CurrentObject;
            if (obj is Config.Texture texture)
            {
                trackWidth.Value = texture.Width;
                trackOffset.Value = texture.Offset;
                textName.Text = texture.Name;
                Render();
            }
        }

        private void TrackWidth_ValueChanged(object sender, EventArgs e)
        {
            var (filename, obj) = CurrentObject;
            if (obj is Config.Texture texture && texture.Width != trackWidth.Value)
            {
                texture.Width = trackWidth.Value;
                Render();
            }

            if (sender != numWidth && (int)numWidth.Value != trackWidth.Value)
                numWidth.Value = trackWidth.Value;
        }

        private void NumWidth_ValueChanged(object sender, EventArgs e)
        {
            if (sender != trackWidth && trackWidth.Value != (int)numWidth.Value)
                trackWidth.Value = (int)numWidth.Value;
        }

        private void TextName_TextChanged(object sender, EventArgs e)
        {
            var (filename, obj) = CurrentObject;
            if (obj is Config.Texture texture)
                texture.Name = textName.Text;
        }

        private void TrackOffset_ValueChanged(object sender, EventArgs e)
        {
            var (filename, obj) = CurrentObject;
            if (obj is Config.Texture texture && texture.Offset != trackOffset.Value)
            {
                texture.Offset = trackOffset.Value;
                Render();
            }

            if (sender != numOffset && (int)numOffset.Value != trackOffset.Value)
                numOffset.Value = trackOffset.Value;
        }

        private void NumOffset_ValueChanged(object sender, EventArgs e)
        {
            if (sender != trackOffset && trackOffset.Value != (int)numOffset.Value)
                trackOffset.Value = (int)numOffset.Value;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveClicked?.Invoke(this, EventArgs.Empty);
        }

        private void NumWidth_Enter(object sender, EventArgs e)
        {
            numWidth.Select(0, numWidth.Text.Length);
        }

        private void ListPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void TrackWidth_KeyDown(object sender, KeyEventArgs e)
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
    }
}
