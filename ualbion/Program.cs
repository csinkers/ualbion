using System.IO;
using System.Reflection;
using UAlbion.Formats;

namespace UAlbion
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseDir = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.Parent.FullName;
            Config config = Config.Load(baseDir);

            var assets = new Assets(config);
            var palette = assets.LoadPalette(2);
            var menuBackground = assets.LoadTexture(AssetType.Picture, 19);

            /*
            Load palettes
            Load GUI sprites
            Show game frame
                Set mode to main menu
            */

        }
    }
}
