using System;
using System.IO;
using System.Windows.Forms;
using UAlbion.Api;
using UAlbion.Config;

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
            var disk = new FileSystem();
            var baseDir = ConfigUtil.FindBasePath(disk);
            var generalConfig = GeneralConfig.Load(Path.Combine(baseDir, "data/config.json"), baseDir, disk);
            var config = AssetConfig.Load(Path.Combine(baseDir, "mods/Base/assets.json"), disk);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            void SaveChanges(object sender, EventArgs e) => config.Save(Path.Combine(baseDir, "mods/Base/assets.json"), disk);

            var core = new ReverserCore(generalConfig, config);
            var form = new MainFrm(core);
            form.SaveClicked += SaveChanges;
            Application.Run(form);
            form.SaveClicked -= SaveChanges;

            SaveChanges(null, EventArgs.Empty);
        }
    }
}
