using System.Text;
using System.Windows.Forms;

namespace UAlbion.Tools.ImageReverser
{
    public partial class TextViewer : UserControl, IAssetViewer
    {
        // readonly ReverserCore _core;
        public TextViewer()
        {
            // _core = core;
            InitializeComponent();
        }

        public void GetAssetDescription(StringBuilder sb)
        {
        }
    }
}
