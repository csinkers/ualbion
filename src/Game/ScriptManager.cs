using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game;

public class ScriptManager : GameComponent
{
    public ScriptManager()
    {
        OnAsync<DoScriptEvent>(Run);
        On<DumpScriptEvent>(Dump);
    }

    AlbionTask Run(DoScriptEvent doScriptEvent)
    {
        var mapManager = Resolve<IMapManager>();

        var events = Assets.LoadScript(doScriptEvent.ScriptId);
        if (events == null)
        {
            Error($"Could not load script {doScriptEvent.ScriptId}");
            return AlbionTask.Complete;
        }

        var nodes = new EventNode[events.Count];

        // Create, link and add all the nodes.
        for (ushort i = 0; i < events.Count;     i++) nodes[i] = new EventNode(i, events[i]);
        for (ushort i = 0; i < events.Count - 1; i++) nodes[i].Next = nodes[i + 1];

        var set = new ScriptEventSet(doScriptEvent.ScriptId, mapManager.Current.MapId.ToMapText(), nodes);
        var source = new EventSource(mapManager.Current.MapId, TriggerType.Default); // TODO: Is there a better trigger type for this?
        var trigger = new TriggerChainEvent(set, 0, source);
        return RaiseAsync(trigger);
    }

    void Dump(DumpScriptEvent dumpScriptEvent)
    {
        var events = Assets.LoadScript(dumpScriptEvent.ScriptId);
        foreach (var e in events)
            Info(e.ToString());
    }
}

[Event("dump_script")]
public class DumpScriptEvent : GameEvent
{
    public DumpScriptEvent(ScriptId scriptId) => ScriptId = scriptId;
    [EventPart("id")] public ScriptId ScriptId { get; }
}
