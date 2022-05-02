using System;
using System.Linq;
using System.Collections.Generic;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Menus;

public class OptionsMenu : ModalDialog
{
    public event EventHandler Closed;

    int _musicVolume;
    int _fxVolume;
    int _combatDelay;

    bool HasLanguageFiles(string language)
        => Resolve<IAssetManager>().IsStringDefined(Base.SystemText.MainMenu_MainMenu, language);

    public OptionsMenu() : base(DialogPositioning.Center) { }

    protected override void Subscribed()
    {
        var languageButtons = new List<IUiElement>();
        void SetLanguage(string language) => Raise(new SetLanguageEvent(language));

        var languages = new List<(string, string)>();
        var modApplier = Resolve<IModApplier>();
        foreach (var kvp in modApplier.Languages.OrderBy(x => x.Value.ShortName))
            if (HasLanguageFiles(kvp.Key))
                languages.Add((kvp.Key, kvp.Value.ShortName));

        foreach (var (language, shortName) in languages)
            languageButtons.Add(new Button(shortName).OnClick(() => SetLanguage(language)));

        var elements = new List<IUiElement>
        {
            new Spacing(156,2),
            new Label(TextId.From(Base.UAlbionString.LanguageLabel)),
            new HorizontalStack(languageButtons),
            new Spacing(0,2),
            new Label((TextId)Base.SystemText.Options_MusicVolume),
            new Slider(() => _musicVolume, x => _musicVolume = x, 0, 127),
            new Spacing(0,2),
            new Label((TextId)Base.SystemText.Options_FXVolume),
            new Slider(() => _fxVolume, x => _fxVolume = x, 0, 127),
            new Spacing(0,2),
            new Label((TextId)Base.SystemText.Options_CombatTextDelay),
            new Slider(() => _combatDelay, x => _combatDelay = x, 1, 50),
            new Spacing(0,2),
            new Button(Base.SystemText.MsgBox_OK).OnClick(SaveAndClose),
            new Spacing(0,2),
        };
        var stack = new VerticalStack(elements);
        AttachChild(new DialogFrame(stack));

        var settings = Resolve<ISettings>();
        _musicVolume = settings.Audio.MusicVolume;
        _fxVolume = settings.Audio.FxVolume;
        _combatDelay = settings.Gameplay.CombatDelay;
    }

    void SaveAndClose()
    {
        var settings = Resolve<ISettings>();
        if (_musicVolume != settings.Audio.MusicVolume) Raise(new SetMusicVolumeEvent(_musicVolume));
        if (_fxVolume != settings.Audio.FxVolume) Raise(new SetFxVolumeEvent(_fxVolume));
        if (_combatDelay != settings.Gameplay.CombatDelay) Raise(new SetCombatDelayEvent(_combatDelay));
        settings.Save();

        Closed?.Invoke(this, EventArgs.Empty);
        Remove();
    }
}