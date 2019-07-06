using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace UAlbion.ImageReverser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string rootPath = @"C:\Depot\Main\ualbion\exported";
            var configPath = Path.Combine(rootPath, "config.json");
            var configText = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<Config>(configText, new ConfigObjectConverter());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            void SaveChanges(object sender, EventArgs args)
            {
                var json = JsonConvert.SerializeObject(config, new ConfigObjectConverter());
                File.WriteAllText(configPath, json);
            }

            var form = new MainFrm(rootPath, config);
            form.SaveClicked += SaveChanges;
            Application.Run(form);
            form.SaveClicked -= SaveChanges;

            SaveChanges(null, EventArgs.Empty);
        }
    }
}
