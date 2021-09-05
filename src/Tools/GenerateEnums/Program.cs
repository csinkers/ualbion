using UAlbion.Api;

namespace UAlbion.CodeGenerator
{
    static class Program
    {
        static void Main()
        {
            // TODO: Add verify mode
            var disk = new FileSystem();
            var jsonUtil = new JsonUtil();
            var assets = new Assets(disk, jsonUtil);
            GenerateAssetIds.Generate(assets);
        }
    }
}
