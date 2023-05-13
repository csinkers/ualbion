using System;
using System.Numerics;
using System.Text;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class ImGuiConsoleLogger : Component, IImGuiWindow
{
    // TODO: Initial size
    readonly byte[] _inputBuffer = new byte[512];
    bool _autoScroll = true;
    bool _scrollToBottom = true;
    bool _focus;

    public string Name { get; }
    public ImGuiConsoleLogger(string name)
    {
        Name = name;
        On<FocusConsoleEvent>(_ => _focus = true);
    }

    public void Draw()
    {
        var window = Resolve<IGameWindow>();
        bool open = true;
        ImGui.Begin(Name, ref open);
        ImGui.SetWindowPos(Vector2.Zero, ImGuiCond.FirstUseEver);
        ImGui.SetWindowSize(new Vector2(window.PixelWidth / 3.0f, window.PixelHeight), ImGuiCond.FirstUseEver);

        // Reserve enough left-over height for 1 separator + 1 input text
        float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        ImGui.BeginChild(
            "ScrollingRegion",
            new Vector2(0, -footerHeightToReserve),
            false,
            ImGuiWindowFlags.HorizontalScrollbar);

        // Display every line as a separate entry so we can change their color or add custom widgets.
        // If you only want raw text you can use ImGui.TextUnformatted(log.begin(), log.end());
        // NB- if you have thousands of entries this approach may be too inefficient and may require user-side clipping
        // to only process visible items. The clipper will automatically measure the height of your first item and then
        // "seek" to display only items in the visible area.
        // To use the clipper we can replace your standard loop:
        //      for (int i = 0; i < Items.Size; i++)
        //   With:
        //      ImGuiListClipper clipper(Items.Size);
        //      while (clipper.Step())
        //         for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
        // - That your items are evenly spaced (same height)
        // - That you have cheap random access to your elements (you can access them given their index,
        //   without processing all the ones before)
        // You cannot this code as-is if a filter is active because it breaks the 'cheap random-access' property.
        // We would need random-access on the post-filtered list.
        // A typical application wanting coarse clipping and filtering may want to pre-compute an array of indices
        // or offsets of items that passed the filtering test, recomputing this array when user changes the filter,
        // and appending newly elements as they are inserted. This is left as a task to the user until we can manage
        // to improve this example code!
        // If your items are of variable height:
        // - Split them into same height items would be simpler and facilitate random-seeking into your list.
        // - Consider using manual call to IsRectVisible() and skipping extraneous decoration from your items.

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4,1)); // Tighten spacing

        var history = Resolve<ILogHistory>();
        history.Access(0, (_, logs) =>
        {
            foreach (var log in logs)
            {
                //if (!Filter.PassFilter(item))
                //    continue;

                // Normally you would store more information in your item than just a string.
                // (e.g. make Items[] an array of structure, store color/type etc.)
                ImGui.PushStyleColor(ImGuiCol.Text, ConsoleColorToRgba(log.Color));
                ImGui.Indent(log.Nesting);
                ImGui.TextUnformatted(log.Message);
                ImGui.Unindent(log.Nesting);
                ImGui.PopStyleColor();
            }
        });

        if (_scrollToBottom || (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
            ImGui.SetScrollHereY(1.0f);
        _scrollToBottom = false;

        ImGui.PopStyleVar();
        ImGui.EndChild();
        ImGui.Separator();

        // Command-line
        bool reclaimFocus = false;
        ImGuiInputTextFlags inputTextFlags =
            ImGuiInputTextFlags.EnterReturnsTrue;
        //  | ImGuiInputTextFlags.CallbackCompletion
        //  | ImGuiInputTextFlags.CallbackHistory;

        if (_focus)
        {
            ImGui.SetKeyboardFocusHere(0);
            _focus = false;
        }

        if (ImGui.InputText("", _inputBuffer, (uint)_inputBuffer.Length, inputTextFlags))
        {
            var logExchange = Resolve<ILogExchange>();
            var command = Encoding.ASCII.GetString(_inputBuffer);
            command = command.Substring(0, command.IndexOf((char)0, StringComparison.Ordinal));
            for (int i = 0; i < command.Length; i++)
                _inputBuffer[i] = 0;

            IEvent parsedEvent = Event.Parse(command, out var error);
            if (parsedEvent != null)
                logExchange.EnqueueEvent(parsedEvent);
            else
                PrintMessage(logExchange, error, LogLevel.Error);

            reclaimFocus = true;
        }

        ImGui.SetItemDefaultFocus();
        if (reclaimFocus)
            ImGui.SetKeyboardFocusHere(-1); // Auto focus previous widget

        ImGui.SameLine();
        ImGui.Checkbox("Scroll", ref _autoScroll);

        ImGui.End();

        if (!open)
            Remove();
    }

    void PrintMessage(ILogExchange logExchange, string message, LogLevel level) 
        => logExchange.Receive(new LogEvent(level, message), this);

    /*
            unsafe int TextEditCallbackStub(ImGuiInputTextCallbackData* data)
            {
                switch (data->EventFlag)
                {
                    case ImGuiInputTextFlags.CallbackCompletion:
                    {
                        // Example of TEXT COMPLETION

                        // Locate beginning of current word
                        byte* wordEnd = data->Buf + data->CursorPos;
                        byte* wordStart = wordEnd;
                        while (wordStart > data->Buf)
                        {
                            byte c = wordStart[-1];
                            if (c == ' ' || c == '\t' || c == ',' || c == ';')
                                break;
                            wordStart--;
                        }

                        // Build a list of candidates
                        List<string> candidates;
                        for (int i = 0; i < Commands.Size; i++)
                            if (Strnicmp(Commands[i], wordStart, (int)(wordEnd - wordStart)) == 0)
                                candidates.pushBack(Commands[i]);

                        if (candidates.Size == 0)
                        {
                            // No match
                            AddLog("No match for \"%.*s\"!\n", (int)(wordEnd - wordStart), wordStart);
                        }
                        else if (candidates.Size == 1)
                        {
                            // Single match. Delete the beginning of the word and replace it entirely so we've got nice casing.
                            data->DeleteChars((int)(wordStart - data->Buf), (int)(wordEnd - wordStart));
                            data->InsertChars(data->CursorPos, candidates[0]);
                            data->InsertChars(data->CursorPos, " ");
                        }
                        else
                        {
                            // Multiple matches. Complete as much as we can..
                            // So inputing "C"+Tab will complete to "CL" then display "CLEAR" and "CLASSIFY" as matches.
                            int matchLen = (int)(wordEnd - wordStart);
                            for (; ; )
                            {
                                int c = 0;
                                bool allCandidatesMatches = true;
                                for (int i = 0; i < candidates.Size && allCandidatesMatches; i++)
                                    if (i == 0)
                                        c = toupper(candidates[i][matchLen]);
                                    else if (c == 0 || c != toupper(candidates[i][matchLen]))
                                        allCandidatesMatches = false;
                                if (!allCandidatesMatches)
                                    break;
                                matchLen++;
                            }

                            if (matchLen > 0)
                            {
                                data->DeleteChars((int)(wordStart - data->Buf), (int)(wordEnd - wordStart));
                                data->InsertChars(data->CursorPos, candidates[0], candidates[0] + matchLen);
                            }

                            // List matches
                            AddLog("Possible matches:\n");
                            for (int i = 0; i < candidates.Size; i++)
                                AddLog("- %s\n", candidates[i]);
                        }

                        break;
                    }
                case ImGuiInputTextFlags.CallbackHistory:
                    {
                        // Example of HISTORY
                        const int prevHistoryPos = HistoryPos;
                        if (data->EventKey == ImGuiKey.UpArrow)
                        {
                            if (HistoryPos == -1)
                                HistoryPos = History.Size - 1;
                            else if (HistoryPos > 0)
                                HistoryPos--;
                        }
                        else if (data->EventKey == ImGuiKey.DownArrow)
                        {
                            if (HistoryPos != -1)
                                if (++HistoryPos >= History.Size)
                                    HistoryPos = -1;
                        }

                        // A better implementation would preserve the data on the current input line along with cursor position.
                        if (prevHistoryPos != HistoryPos)
                        {
                            const char* historyStr = (HistoryPos >= 0) ? History[HistoryPos] : "";
                            data->DeleteChars(0, data->BufTextLen);
                            data->InsertChars(0, historyStr);
                        }
                    }
                }
                return 0;
            }
            */

    static Vector4 ConsoleColorToRgba(ConsoleColor color) => color switch
    {
        ConsoleColor.White => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        ConsoleColor.Cyan => new Vector4(0.3f, 1.0f, 1.0f, 1.0f),
        ConsoleColor.Red => new Vector4(1.0f, 0.3f, 0.3f, 1.0f),
        ConsoleColor.Yellow => new Vector4(1.0f, 1.0f, 0.3f, 1.0f),
        _ => new Vector4(0.85f, 0.85f, 0.85f, 1.0f),
    };
}
