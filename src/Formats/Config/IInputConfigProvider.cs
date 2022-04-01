using System;

namespace UAlbion.Formats.Config;

public interface IInputConfigProvider
{
    InputConfig Input { get; }
    event EventHandler<EventArgs> InputChanged;
}