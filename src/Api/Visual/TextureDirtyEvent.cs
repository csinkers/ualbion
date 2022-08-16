using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Api.Visual;

public record TextureDirtyEvent(ITexture Texture) : IVerboseEvent
{
    public void Format(IScriptBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        builder.Add(ScriptPartType.EventName, ToString());
    }
}