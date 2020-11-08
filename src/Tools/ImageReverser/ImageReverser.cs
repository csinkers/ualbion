using System;
using System.IO;
using System.Windows.Forms;
using UAlbion.Config;
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
            var baseDir = ConfigUtil.FindBasePath();
            var generalConfig = GeneralConfig.Load(Path.Combine(baseDir, "data/config.json"), baseDir);
            var config = AssetConfig.Load(baseDir);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            void SaveChanges(object sender, EventArgs e) => config.Save(baseDir);

            var core = new ReverserCore(generalConfig, config);
            var form = new MainFrm(core);
            form.SaveClicked += SaveChanges;
            Application.Run(form);
            form.SaveClicked -= SaveChanges;

            SaveChanges(null, EventArgs.Empty);
        }
    }
}
