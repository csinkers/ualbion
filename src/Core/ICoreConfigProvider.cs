using System;

namespace UAlbion.Core;

public interface ICoreConfigProvider
{
    CoreConfig Core { get; }
    event EventHandler<EventArgs> CoreChanged;
}