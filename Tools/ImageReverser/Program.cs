using System;
using System.IO;
using System.Reflection;
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
            var baseDir = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.Parent.FullName;
            var dataDir = Path.Combine(baseDir, @"data");
            var configPath = Path.Combine(dataDir, @"config.json");
            Config config;
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<Config>(configText, new ConfigObjectConverter());
            }
            else
            {
                config = new Config
                {
                    BaseXldPath = @"..\albion_sr\CD\XLD",
                    ExportedXldPath = @"..\exported"
                };
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            void SaveChanges(object sender, EventArgs args)
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.Converters.Add(new ConfigObjectConverter());
                serializerSettings.Formatting = Formatting.Indented;
                var json = JsonConvert.SerializeObject(config, serializerSettings);
                File.WriteAllText(configPath, json);
            }

            var form = new MainFrm(dataDir, config);
            form.SaveClicked += SaveChanges;
            Application.Run(form);
            form.SaveClicked -= SaveChanges;

            SaveChanges(null, EventArgs.Empty);
        }
    }
}
