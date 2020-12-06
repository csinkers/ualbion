using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;

namespace UAlbion.Tools.ImageReverser
{
    public partial class ImageViewer : UserControl, IAssetViewer
    {
        const int Magnify = 3;
        readonly ReverserCore _core;
        readonly Timer _timer;
        readonly DateTime _startTime;
        AlbionSprite _logicalSprite;
        AlbionSprite _visualSprite;

        public ImageViewer(ReverserCore core)
        {
            _startTime = DateTime.Now;
            _core = core;
            _timer = new Timer { Interval = 250 };
            _timer.Tick += OnTimerTick;
            _core.SelectionChanged += CoreOnSelectionChanged;
            _timer.Start();

            InitializeComponent();

            foreach (var palette in _core.Palettes)
                chkListPalettes.Items.Add(palette);
            chkListPalettes.SelectedIndex = 0;
        }

        void CoreOnSelectionChanged(object sender, SelectedAssetChangedArgs e)
        {
            var asset = _core.SelectedObject;
            if (asset == null)
                return;
            SyncSelectedPalettes();

            trackWidth.Value = asset.EffectiveWidth == 0 ? 1 : asset.EffectiveWidth;
            trackFrame.Value = 0;
            Render();

            if (_logicalSprite != null)
            {
                trackFrameCount.Value = _logicalSprite.Frames.Count;
                if (asset.File.Loader == FixedSizeSpriteLoader.TypeString &&
                    asset.Height != null &&
                    _logicalSprite.Frames[0].Height != asset.Height)
                {
                    asset.Height = _logicalSprite.Frames[0].Height;
                }
            }
        }

        public void GetAssetDescription(StringBuilder sb)
        {
            if (_logicalSprite != null)
            {
                sb.AppendLine($"Logical Frame Count: {_logicalSprite.Frames.Count}");
                sb.AppendLine($"Logical Sprite Width: {_logicalSprite.Width}");
                sb.AppendLine($"Logical Sprite Height: {_logicalSprite.Height}");

                sb.AppendLine($"Logical Frame Width: {_logicalSprite.Frames[trackFrame.Value].Width}");
                sb.AppendLine($"Logical Frame Height: {_logicalSprite.Frames[trackFrame.Value].Height}");
                sb.AppendLine($"Logical Frame X: {_logicalSprite.Frames[trackFrame.Value].X}");
                sb.AppendLine($"Logical Frame Y: {_logicalSprite.Frames[trackFrame.Value].Y}");
            }

            sb.AppendLine();

            if (_visualSprite != null)
            {
                sb.AppendLine($"Visual Frame Count: {_visualSprite.Frames.Count}");
                sb.AppendLine($"Visual Sprite Width: {_visualSprite.Width}");
                sb.AppendLine($"Visual Sprite Height: {_visualSprite.Height}");

                sb.AppendLine($"Visual Frame Width: {_visualSprite.Frames[trackFrame.Value].Width}");
                sb.AppendLine($"Visual Frame Height: {_visualSprite.Frames[trackFrame.Value].Height}");
                sb.AppendLine($"Visual Frame X: {_visualSprite.Frames[trackFrame.Value].X}");
                sb.AppendLine($"Visual Frame Y: {_visualSprite.Frames[trackFrame.Value].Y}");
            }
        }

        void OnTimerTick(object sender, EventArgs e)
        {
            if(_visualSprite?.Frames.Count > 1)
            {
                var frame = trackFrame.Value;
                frame++;

                var filename = _core.SelectedObject?.File?.Filename;
                if ((filename ?? "").Contains("MONGFX")) // Skip odd frames for monster graphics
                    frame++;

                frame %= _visualSprite.Frames.Count;
                trackFrame.Value = frame;
            }

            Render();
        }

        void Render()
        {
            var asset = _core.SelectedObject;
            if (asset == null)
                return;

            Bitmap bmp;
            if (IsSprite(asset.File))
            {
                if (asset.File.Filename != _logicalSprite?.Name)
                {
                    // Ugh
                    bool isRotated = asset.File.Transposed ?? false;
                    asset.File.Transposed = false;
                    _logicalSprite = LoadSprite(_core.GetRawPath(asset), asset);
                    asset.File.Transposed = isRotated;

                    _visualSprite = isRotated ? LoadSprite(asset.File.Filename, asset) : _logicalSprite;
                }

                if (_logicalSprite == null)
                    return;

                trackFrameCount.Maximum = _logicalSprite.Height;
                numFrameCount.Maximum = trackFrameCount.Maximum;
                trackFrame.Maximum = _logicalSprite.Frames.Count - 1;
                numFrame.Maximum = trackFrame.Maximum;

                if (trackWidth.Value == 1)
                    trackWidth.Value = _logicalSprite.Width;

                var palette = (AlbionPalette)(chkListPalettes.SelectedItem ?? chkListPalettes.Items[0]);
                uint[] curPalette = palette.GetPaletteAtTime((int)((DateTime.Now - _startTime).TotalSeconds * 4));

                var width = _visualSprite.Width;
                var frame = Math.Max(0, trackFrame.Value);
                bmp = GenerateBitmap(_visualSprite, frame, width, Magnify, curPalette);
            }
            //else if (asset.Layer == FileFormat.Map2D)
            //{
            //    _logicalSprite = null;
            //    _visualSprite = null;
            //    bmp = new Bitmap(1, 1);
            //}
            else
            {
                _logicalSprite = null;
                _visualSprite = null;
                bmp = new Bitmap(1, 1);
            }

            canvas.Image = bmp;
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

                                p[0] = (byte)((palette[color] & 0x00ff0000) >> 16);
                                p[1] = (byte)((palette[color] & 0x0000ff00) >> 8);
                                p[2] = (byte)(palette[color] & 0x000000ff);
                            }
                        }
                    }
                }
            }
            finally { bmp.UnlockBits(d); }

            return bmp;
        }

        void TrackWidth_ValueChanged(object sender, EventArgs e)
        {
            var asset = _core.SelectedObject;
            if (asset == null)
                return;

            if (!asset.File.Width.HasValue &&
                asset.File.Format == FixedSizeSpriteLoader.TypeString &&
                asset.Width != trackWidth.Value)
            {
                asset.Width = trackWidth.Value;
                _logicalSprite = null; // Force sprite reload
                _visualSprite = null;
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

        void TrackFrameCount_ValueChanged(object sender, EventArgs e)
        {
            var asset = _core.SelectedObject;
            if (_logicalSprite != null && asset != null)
            {
                int? newHeight =
                    trackFrameCount.Value <= 1
                        ? (int?)null
                        : _logicalSprite.Height / trackFrameCount.Value;

                if (!asset.File.Height.HasValue &&
                    asset.File.Format == FixedSizeSpriteLoader.TypeString &&
                    asset.Height != newHeight)
                {
                    asset.Height = newHeight;
                    _logicalSprite = null; // Force sprite reload
                    _visualSprite = null;
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

        void ChkAnimate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAnimate.Checked) _timer.Start();
            else _timer.Stop();
        }

        void ChkListPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        void ChkListPalettes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var asset = _core.SelectedObject;
            if (asset == null)
                return;

            var palette = (AlbionPalette)chkListPalettes.Items[e.Index];
            var paletteId = PaletteId.FromUInt32(palette.Id);
            if (e.NewValue == CheckState.Checked)
            {
                asset.PaletteHint = paletteId.Id;
                // TODO: Uncheck all others
            }
            else
            {
                if (asset.PaletteHint == paletteId.Id)
                    asset.PaletteHint = null;
            }

            _core.TriggerAssetChanged(asset);
        }

        void SyncSelectedPalettes()
        {
            var asset = _core.SelectedObject;
            if (asset == null)
                return;

            for (int index = 0; index < chkListPalettes.Items.Count; index++)
            {
                var item = (AlbionPalette)chkListPalettes.Items[index];
                var paletteId = PaletteId.FromUInt32(item.Id);
                chkListPalettes.SetItemChecked(index, asset.PaletteHint == paletteId.Id);
            }

            if (chkListPalettes.SelectedIndex != -1)
                if (!chkListPalettes.GetItemChecked(chkListPalettes.SelectedIndex) && chkListPalettes.CheckedIndices.Count > 0)
                    chkListPalettes.SelectedIndex = chkListPalettes.CheckedIndices[0];
        }

        static bool IsSprite(AssetFileInfo info)
        {
            switch (info.Loader)
            {
                case "UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.FontSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.HeaderBasedSpriteLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.InterlacedBitmapLoader, UAlbion.Formats":
                case "UAlbion.Formats.Parsers.SlabLoader, UAlbion.Formats":
                    return true;
                default: return false;
            }
        }

        AlbionSprite LoadSprite(string filename, AssetInfo conf)
        {
            using var stream = File.OpenRead(Path.Combine(_core.BaseExportDirectory, filename));
            using var br = new BinaryReader(stream);
            using var ar = new AlbionReader(br, stream.Length);
            return (AlbionSprite)GetLoader(conf).Serdes(null, conf, AssetMapping.Global, ar);
        }

        static IAssetLoader GetLoader(AssetInfo conf)
        {
            var type = Type.GetType(conf.File.Loader);
            if(type == null)
                throw new InvalidOperationException($"Could not find loader type \"{conf.File.Loader}\"");

            var constructor = type.GetConstructor(new Type[0]);
            if(constructor == null)
                throw new InvalidOperationException($"Could not find parameterless constructor for loader type \"{type}\"");

            return (IAssetLoader)constructor.Invoke(new object[0]);
        }
    }
}
