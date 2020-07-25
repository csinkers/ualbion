namespace UAlbion.Formats.Config
{
    public interface IGeneralConfig
    {
        string BasePath { get; }
        string BaseDataPath { get; }
        string XldPath { get; }
        string ExePath { get; }
        string SavePath { get; }
        string ExportedXldPath { get; }
    }
}
