using System.Collections.Generic;
using UAlbion.Api.Eventing;

namespace UAlbion.Scripting;

public interface IEventFormatter
{
    void Format(IScriptBuilder builder, IEvent e);
    DecompilationResult Decompile<T>(
        IList<T> events,
        IEnumerable<ushort> chains,
        IEnumerable<ushort> additionalEntryPoints,
        int indent = 0) where T : IEventNode;
}