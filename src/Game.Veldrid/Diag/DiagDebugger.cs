using System;
using ImGuiColorTextEditNet;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Formats;

namespace UAlbion.Game.Veldrid.Diag;

public class DiagDebugger : Component
{
    readonly DiagNewBreakpoint _newBreakpoint;
    readonly TextEditor _editor;

    int _currentContextIndex;
    string[] _contextNames = Array.Empty<string>();

    int _currentBreakpointIndex;
    string[] _breakpointNames = Array.Empty<string>();

    public DiagDebugger()
    {
        _newBreakpoint = AttachChild(new DiagNewBreakpoint());
        _editor = new TextEditor
        {
            IsReadOnly = true,
            SyntaxHighlighter = new AlbionSyntaxHighlighter()
        };
    }

    public void Render()
    {
        ImGui.TextUnformatted(Context.ToString());

        var chainManager = Resolve<IEventManager>();

        if (ImGui.Button("Add BP"))
            ImGui.OpenPopup("New Breakpoint");

        _newBreakpoint.Render();

        if (ImGui.Button("Del BP") && _currentBreakpointIndex < chainManager.Breakpoints.Count)
            chainManager.RemoveBreakpoint(_currentBreakpointIndex);

        if (_breakpointNames.Length != chainManager.Breakpoints.Count)
            _breakpointNames = new string[chainManager.Breakpoints.Count];

        for (int i = 0; i < _breakpointNames.Length; i++)
            _breakpointNames[i] = chainManager.Breakpoints[i].ToString();

        if (ImGui.ListBox("Breakpoints", ref _currentBreakpointIndex, _breakpointNames, _breakpointNames.Length))
        {
        }

        if (_contextNames.Length != chainManager.Contexts.Count)
            _contextNames = new string[chainManager.Contexts.Count];

        for (int i = 0; i < _contextNames.Length; i++)
            _contextNames[i] = chainManager.Contexts[i].ToString();

        ImGui.ListBox("Active Contexts", ref _currentContextIndex, _contextNames, _contextNames.Length);

        if (_currentContextIndex >= 0 && _currentContextIndex < _contextNames.Length)
        {
            var context = chainManager.Contexts[_currentContextIndex];
            var set = context.EventSet;
            if (set.Decompiled == null)
            {
                var assets = Resolve<IAssetManager>();
                var eventFormatter = new EventFormatter(assets.LoadString, context.EventSet.TextId);
                set.Decompiled = eventFormatter.Decompile(set.Events, set.Chains, set.ExtraEntryPoints);
                var code = set.Decompiled.Script;
                _editor.Text = code;
            }

            _editor.Render("Script");

            /* ImGui.InputTextMultiline("Script",
                ref code,
                (uint)code.Length,
                new Vector2(-1, ImGui.GetTextLineHeight() * 64),
                ImGuiInputTextFlags.ReadOnly,
                CodeEditorCallback); */
        }
    }
}