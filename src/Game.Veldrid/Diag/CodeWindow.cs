using System;
using ImGuiColorTextEditNet;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Veldrid.Diag;

/*
| [x] Break on next
| [Step Over] [Step In] [Step Out]
| [Tabs for active scripts]
|#if (prompt_user bla) {
|     map_text 231 ; Some text
| } else {
|     teleport 200 60 50
| }
|----------------------------------------------------
| Watch Window    |       | Call Stack
| Switch.123      | true  | current script context
| Ticker.10       | 5     | previous etc
| - NpcSheet.Sira |       |
| |-- Name        | Sira  |
| |-- Inventory   |       |
| ||-- Gold       | 23.5  |
| ||+- etc        |       |

*/

public class CodeWindow : Component, IImGuiWindow
{
    readonly Func<EventContext> _getContext;
    readonly TextEditor _editor;
    readonly string _name;

    public CodeWindow(int id, Func<EventContext> getContext)
    {
        // var chainManager = Resolve<IEventManager>();
        // var context = chainManager.Contexts[_currentContextIndex];

        _getContext = getContext ?? throw new ArgumentNullException(nameof(getContext));
        _name = $"Code###Code{id}";
        _editor = new TextEditor
        {
            IsReadOnly = true,
            SyntaxHighlighter = new AlbionSyntaxHighlighter()
        };
    }

    public void Draw()
    {
        var context = _getContext();
        if (context == null)
            return;

        ImGui.Begin(_name);
        ImGui.TextUnformatted(Context.ToString());

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
        ImGui.End();
    }
}