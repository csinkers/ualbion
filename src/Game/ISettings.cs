using UAlbion.Api.Settings;

namespace UAlbion.Game;

public interface ISettings : IVarSet
{
    void Save();
}