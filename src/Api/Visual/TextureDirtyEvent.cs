using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Api.Visual;

public record TextureDirtyEvent(ITexture Texture) : IVerboseEvent
{
    public void Format(IScriptBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Add(ScriptPartType.EventName, ToString());
    }
}
