using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;

namespace UAlbion.Game;

public interface ILogHistory
{
    void Access<T>(T context, Action<T, IReadOnlyCollection<LogEventArgs>> operation);
}