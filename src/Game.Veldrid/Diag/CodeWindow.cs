using ImGuiColorTextEditNet;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;
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
    readonly TextEditor _editor;
    public string Name { get; }

    public CodeWindow(string name)
    {
        Name = name;
        _editor = new TextEditor
        {
            Options = { IsReadOnly = true },
            SyntaxHighlighter = new AlbionSyntaxHighlighter()
        };
    }

    public void Draw()
    {
        var chainManager = Resolve<IEventManager>();
        var context = chainManager.CurrentDebugContext;

        bool open = true;
        ImGui.Begin(Name, ref open);
        if (context != null)
            DrawContext(context);

        ImGui.End();

        if (!open)
            Remove();
    }

    void DrawContext(EventContext context)
    {
        ImGui.TextUnformatted(Context.ToString());

        var set = context.EventSet;
        if (set.Decompiled == null)
        {
            var assets = Resolve<IAssetManager>();
            var eventFormatter = new EventFormatter(assets.LoadString, context.EventSet.TextId);
            set.Decompiled = eventFormatter.Decompile(set.Events, set.Chains, set.ExtraEntryPoints);
            var code = set.Decompiled.Script;
            _editor.AllText = code;
            // TODO: Add breakpoints
        }

        _editor.Render("Script");
    }
}