using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationTopicWindow : ModalDialog
{
    readonly List<WordId> _currentWords = new();

    public ConversationTopicWindow(int depth) : base(DialogPositioning.Center, depth)
    {
        // TODO: Keyboard support
        On<UiRightClickEvent>(e =>
        {
            e.Propagating = false;
            OnWordSelected(WordId.None);
        });

        On<DismissMessageEvent>(_ => OnWordSelected(WordId.None));
        On<EnterWordEvent>(_ => ShowWordEntryPrompt());
        On<RespondEvent>(e =>
        {
            int index = e.Option - 1;
            if (_currentWords.Count <= e.Option)
                return;

            OnWordSelected(_currentWords[index]);
        });
    }

    public event EventHandler<WordId> WordSelected;
    void OnWordSelected(WordId e) => WordSelected?.Invoke(this, e);

    public void SetOptions(IDictionary<WordId, WordStatus> words)
    {
        RemoveAllChildren();

        var elements = new List<IUiElement>();
        var lookup = Resolve<IWordLookup>();
        var language = GetVar(UserVars.Gameplay.Language);

        _currentWords.Clear();
        var wordButtons = 
            words
            .OrderBy(x => x.Value)
            .ThenBy(x => lookup.GetText(x.Key, language))
            .Select(x =>
            {
                _currentWords.Add(x.Key);
                var color = x.Value switch
                {
                    WordStatus.Mentioned => Base.Ink.Yellow,
                    WordStatus.Discussed => Base.Ink.White,
                    _ => Base.Ink.Gray,
                };

                var textElement = (IUiElement)new UiTextBuilder(x.Key).Ink(color);
                return (IUiElement)new Button(textElement)
                {
                    Theme = ButtonTheme.Frameless
                }.OnClick(() => OnWordSelected(x.Key));
            }
        ).ToArray();

        if (wordButtons.Any())
        {
            elements.Add(new GroupingFrame(new VerticalStack(wordButtons)));
            elements.Add(new Spacing(0, 3));
        }

        elements.Add(new Button(Base.SystemText.MsgBox_EnterWord).OnClick(ShowWordEntryPrompt));

        AttachChild(new DialogFrame(new Padding(new VerticalStack(elements), 3))
        {
            Background = DialogFrameBackgroundStyle.MainMenuPattern
        });
    }

    void ShowWordEntryPrompt() =>
        RaiseAsync(new TextPromptEvent(), wordString =>
        {
            var wordLookup = Resolve<IWordLookup>();
            var wordId = wordLookup.Parse(wordString);
            OnWordSelected(wordId);
        });
}