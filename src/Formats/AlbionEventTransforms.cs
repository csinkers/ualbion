using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats;

public static class AlbionEventTransforms
{
    public static IEvent TransformForCompiler(IEvent e, TextId stringContext)
    {
        if (e is ScriptTextEvent text)
            return new MapTextEvent(stringContext, text.TextId, text.Location, text.Speaker);

        return e;
    }

    public static IEvent TransformForDecompiler(IEvent e)
    {
        if (e is MapTextEvent text)
            return new ScriptTextEvent(text.SubId, text.Location, text.Speaker);

        return e;
    }
}