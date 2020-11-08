namespace UAlbion.Formats.Config
{
    public interface IGeneralConfig
    {
        string BasePath { get; }
        string XldPath { get; }
        string ExePath { get; }
        string SavePath { get; }
        string ExportedXldPath { get; }
        string SettingsPath { get; }
        string CoreConfigPath { get; }
        string GameConfigPath { get; }
        string BaseAssetsPath { get; }
        string ModPath { get; }
    }
}
