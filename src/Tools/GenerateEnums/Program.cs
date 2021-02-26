namespace UAlbion.CodeGenerator
{
    static class Program
    {
        static void Main()
        {
            // TODO: Add verify mode
            var assets = new Assets();
            GenerateAssetIds.Generate(assets);
        }
    }
}
