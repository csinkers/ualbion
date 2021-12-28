using System.Collections.Generic;

namespace UAlbion.Game.Veldrid.Audio;

public interface IAudioManager
{
    IList<string> ActiveSounds { get; }
}