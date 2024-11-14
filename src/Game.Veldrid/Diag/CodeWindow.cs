using System;
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

public class ScriptWindow : GameComponent, IImGuiWindow
{
    readonly TextEditor _editor;
    public string Name { get; }

    public ScriptWindow(string name)
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
        ImGui.TextUnformatted(context.ToString());

        var set = context.EventSet;
        if (set.Decompiled == null)
        {
            var eventFormatter = new EventFormatter(Assets.LoadStringSafe, context.EventSet.StringSetId);
            set.Decompiled = eventFormatter.Decompile(set.Events, set.Chains, set.ExtraEntryPoints);
            var code = set.Decompiled.Script.AsSpan();
            _editor.AllText = "";

            foreach (var part in set.Decompiled.Parts)
            {
                PaletteIndex color = part.Type switch
                {
                    ScriptPartType.Text           => PaletteIndex.Default,
                    ScriptPartType.Keyword        => PaletteIndex.Keyword,
                    ScriptPartType.EventName      => PaletteIndex.KnownIdentifier,
                    ScriptPartType.Identifier     => PaletteIndex.Identifier,
                    ScriptPartType.Number         => PaletteIndex.Number,
                    ScriptPartType.Operator       => PaletteIndex.Punctuation,
                    ScriptPartType.Label          => PaletteIndex.Identifier,
                    ScriptPartType.StringConstant => PaletteIndex.String,
                    ScriptPartType.Comment        => PaletteIndex.Comment,
                    ScriptPartType.Error          => PaletteIndex.ErrorMarker,
                    _ => PaletteIndex.Default
                };

                _editor.Append(code[part.Range.Start..part.Range.End], color);
            }
            // TODO: Add breakpoints
        }
        // _editor.Selection.HighlightedLine =

        _editor.Render("Script");
    }
}
