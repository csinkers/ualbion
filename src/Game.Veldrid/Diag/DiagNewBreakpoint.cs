using System;
using System.Text;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Diag;

namespace UAlbion.Game.Veldrid.Diag;

public class DiagNewBreakpoint : Component
{
    readonly string[] _triggerNames;
    readonly TriggerType?[] _triggerTypes;
    readonly byte[] _targetBuf = new byte[80];
    // readonly byte[] _eventTypeBuf = new byte[256];
    readonly byte[] _eventIdBuf = new byte[6];
    int _curTriggerIndex;

    public DiagNewBreakpoint()
    {
        var values = Enum.GetValues<TriggerType>();
        _triggerNames = new string[values.Length + 1];
        _triggerTypes = new TriggerType?[values.Length + 1];

        _triggerNames[0] = "None";
        _triggerTypes[0] = null;

        for (int i = 0; i < values.Length; i++)
        {
            _triggerNames[i + 1] = values[i].ToString();
            _triggerTypes[i + 1] = values[i];
        }
    }

    public void Render()
    {
        bool open = true;
        if (!ImGui.BeginPopupModal("New Breakpoint", ref open)) 
            return;

        ImGui.Combo("Trigger", ref _curTriggerIndex, _triggerNames, _triggerNames.Length);
        ImGui.InputText("Target", _targetBuf, (uint)_targetBuf.Length); // TODO: Validation
        // ImGui.InputText("Event", _eventTypeBuf, (uint)_eventTypeBuf.Length); // TODO: Validation
        ImGui.InputText("EventId", _eventIdBuf, (uint)_eventIdBuf.Length); // TODO: Validation

        if (ImGui.Button("Done"))
        {
            var triggerType = _triggerTypes[_curTriggerIndex];
            var targetStr = Encoding.UTF8.GetString(_targetBuf);
            var target = AssetId.Parse(targetStr);

            // var eventTypeStr = Encoding.UTF8.GetString(_eventTypeBuf);
            // var eventType = eventTypeStr;

            var eventIdStr = Encoding.UTF8.GetString(_eventIdBuf);
            var eventId = ushort.TryParse(eventIdStr, out var rawEventId) ? (ushort?)rawEventId : null;

            var bp = new Breakpoint(triggerType, target, eventId);

            var chainManager = Resolve<IEventManager>();
            chainManager.AddBreakpoint(bp);
            ImGui.CloseCurrentPopup();
        }

        if (ImGui.Button("Cancel"))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }
}