using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Tools.ImageReverser
{
    public partial class ImageViewer : UserControl, IAssetViewer
    {
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
                if (asset.Parent.Format == FileFormat.FixedSizeSprite &&
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

                var filename = _core.SelectedObject?.Filename;
                if ((filename ?? "").Contains("MONGFX")) // Skip odd frames for monster graphics
                    frame++;

                frame %= _visualSprite.Frames.Count;
                trackFrame.Value = frame;
            }

            Render();
        }

        void Render()
        {
            const int magnify = 3;
            var asset = _core.SelectedObject;
            if (asset == null)
                return;

            Bitmap bmp;
            if (IsSprite(asset.Parent.Format))
            {
                if (asset.Filename != _logicalSprite?.Name)
                {
                    // Ugh
                    bool isRotated = asset.Parent.Transposed ?? false;
                    asset.Parent.Transposed = false;
                    _logicalSprite = LoadSprite(asset.Filename, asset);
                    asset.Parent.Transposed = isRotated;

                    _visualSprite = isRotated ? LoadSprite(asset.Filename, asset) : _logicalSprite;
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
                bmp = GenerateBitmap(_visualSprite, frame, width, magnify, curPalette);
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
                                p[2] = (byte)((palette[color] & 0x000000ff) >> 0);
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

            if (!asset.Parent.Width.HasValue && 
                asset.Parent.Format == FileFormat.FixedSizeSprite && 
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

                if (!asset.Parent.Height.HasValue && 
                    asset.Parent.Format == FileFormat.FixedSizeSprite && 
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
            if (e.NewValue == CheckState.Checked)
            {
                if (!asset.PaletteHints.Contains(palette.Id))
                    asset.PaletteHints.Add(palette.Id);
            }
            else
            {
                asset.PaletteHints.Remove(palette.Id);
            }

            _core.TriggerAssetChanged(asset);
        }

        void SyncSelectedPalettes()
        {
            var asset = _core.SelectedObject;
            if (asset == null)
                return;

            if (asset.PaletteHints == null)
                asset.PaletteHints = new List<int>();

            for (int index = 0; index < chkListPalettes.Items.Count; index++)
            {
                var item = (AlbionPalette)chkListPalettes.Items[index];
                chkListPalettes.SetItemChecked(index, asset.PaletteHints.Contains(item.Id));
            }

            if (chkListPalettes.SelectedIndex != -1)
                if (!chkListPalettes.GetItemChecked(chkListPalettes.SelectedIndex) && chkListPalettes.CheckedIndices.Count > 0)
                    chkListPalettes.SelectedIndex = chkListPalettes.CheckedIndices[0];
        }

        static bool IsSprite(FileFormat type)
        {
            switch (type)
            {
                // case FileFormat.InterlacedBitmap:
                case FileFormat.AmorphousSprite:
                case FileFormat.FixedSizeSprite:
                case FileFormat.Font:
                case FileFormat.HeaderPerSubImageSprite:
                case FileFormat.MapData:
                case FileFormat.SingleHeaderSprite:
                case FileFormat.Slab:
                    return true;
                default: return false;
            }
        }

        AlbionSprite LoadSprite(string filename, FullAssetInfo conf)
        {
            using var stream = File.OpenRead(Path.Combine(_core.BaseExportDirectory, filename));
            using var br = new BinaryReader(stream);
            return (AlbionSprite)GetLoader(conf).Load(br, stream.Length, filename, conf);
        }

        static IAssetLoader GetLoader(FullAssetInfo conf) =>
            (conf.Parent.Format) switch
            {
            FileFormat.AmorphousSprite => new AmorphousSpriteLoader(),
            FileFormat.FixedSizeSprite => new FixedSizeSpriteLoader(),
            FileFormat.Font => new FixedSizeSpriteLoader(),
            FileFormat.HeaderPerSubImageSprite => new HeaderBasedSpriteLoader(),
            FileFormat.MapData =>new FixedSizeSpriteLoader(),
            FileFormat.SingleHeaderSprite =>new HeaderBasedSpriteLoader(),
            FileFormat.Slab => new FixedSizeSpriteLoader(),
            _ => null
            };
    }
}
