using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationTopicWindow : ModalDialog
{
    readonly List<WordId> _currentWords = new();
    AlbionTaskCore<WordId> _source;

    public ConversationTopicWindow(int depth) : base(DialogPositioning.Center, depth)
    {
        // TODO: Keyboard support
        On<UiRightClickEvent>(e =>
        {
            e.Propagating = false;
            OnWordSelected(WordId.None);
        });

        On<DismissMessageEvent>(_ => OnWordSelected(WordId.None));
        OnQueryAsync<EnterWordEvent, WordId>(_ => PromptForWord());
        On<RespondEvent>(e =>
        {
            int index = e.Option - 1;
            if (_currentWords.Count <= e.Option)
                return;

            OnWordSelected(_currentWords[index]);
        });
    }

    void OnWordSelected(WordId e)
    {
        IsActive = false;

        var source = _source;
        _source = null;
        source.SetResult(e);
    }

    public AlbionTask<WordId> GetWord(IDictionary<WordId, WordStatus> words)
    {
        if (_source != null)
            throw new InvalidOperationException("Tried to get a word while another request was in progress");

        RemoveAllChildren();
        IsActive = true;
        _source = new AlbionTaskCore<WordId>("ConversationTopicWindow.GetWord");

        var elements = new List<IUiElement>();
        var lookup = Resolve<IWordLookup>();
        var language = ReadVar(V.User.Gameplay.Language);

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

        if (wordButtons.Length > 0)
        {
            elements.Add(new GroupingFrame(new VerticalStacker(wordButtons)));
            elements.Add(new Spacing(0, 3));
        }

        elements.Add(new Button(Base.SystemText.MsgBox_EnterWord).OnClick(() =>
        {
            var task = PromptForWord();
            task.OnCompleted(() => OnWordSelected(task.GetResult()));
        }));

        AttachChild(new DialogFrame(new Padding(new VerticalStacker(elements), 3)) { Background = DialogFrameBackgroundStyle.MainMenuPattern });

        return _source.Task;
    }

    async AlbionTask<WordId> PromptForWord()
    {
        var wordString = await RaiseQueryAsync(new TextPromptEvent());
        var wordLookup = Resolve<IWordLookup>();
        return wordLookup.Parse(wordString);
    }
}