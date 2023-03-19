using System.IO;
using UAlbion.Api;

namespace UAlbion.CodeGenerator;

static class Program
{
    const string AssetIdTypesJsonPath = "src/Formats/AssetIdTypes.json";
    const string DestinationPath = "src/Formats/Ids";
    const string DestinationNamespace = "UAlbion.Formats.Ids";
    const string ModName = "Base";

    static void Main()
    {
        // TODO: Add verify mode
        var disk = new FileSystem(Directory.GetCurrentDirectory());
        var jsonUtil = new JsonUtil();
        var assets = new Assets("ualbion-codegen", disk, jsonUtil, AssetIdTypesJsonPath, ModName);
        GenerateAssetIds.Generate(assets, DestinationPath, DestinationNamespace);
    }
}