using System;
using System.IO;
using System.Media;
using System.Text;
using System.Windows.Forms;
using UAlbion.Config;

namespace UAlbion.Tools.ImageReverser
{
    public partial class SoundViewer : UserControl, IAssetViewer
    {
        readonly ReverserCore _core;
        readonly SoundPlayer _player;

        public SoundViewer(ReverserCore core)
        {
            _player = new SoundPlayer();
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _core.SelectionChanged += CoreOnSelectionChanged;
            InitializeComponent();
        }

        void CoreOnSelectionChanged(object sender, SelectedAssetChangedArgs e)
        {
            var asset = e.SelectedObject;
            //if (asset.Format == FileFormat.SampleLibrary)
            if (asset?.Format == FileFormat.AudioSample)
            {
                var stream = File.OpenRead(Path.Combine(_core.BaseExportDirectory, asset.Parent.Filename));
                _player.Stream = stream;
                _player.Play();
            }
        }

        public void GetAssetDescription(StringBuilder sb)
        {
        }
    }
}
