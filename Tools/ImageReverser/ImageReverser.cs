using System;
using System.Windows.Forms;
using UAlbion.Formats;
using UAlbion.Formats.Config;

namespace UAlbion.Tools.ImageReverser
{
    static class ImageReverser
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var baseDir = FormatUtil.FindBasePath();
            var config = FullAssetConfig.Load(baseDir);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            void SaveChanges(object sender, EventArgs e) => config.Save();

            var form = new MainFrm(config);
            form.SaveClicked += SaveChanges;
            Application.Run(form);
            form.SaveClicked -= SaveChanges;

            SaveChanges(null, EventArgs.Empty);
        }
    }
}
