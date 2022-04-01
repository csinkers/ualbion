using UAlbion.Core;
using UAlbion.Formats.Config;

namespace UAlbion.Game;

public interface IConfigProvider :
    ICoreConfigProvider,
    IGameConfigProvider,
    IInputConfigProvider
{
}