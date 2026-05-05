using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory;

public class ConditionsListPanel : UiElement
{
    static readonly (PlayerConditions Flag, Base.SystemText TextId)[] _conditionMap =
    [
        (PlayerConditions.Unconscious, Base.SystemText.Condition_Unconscious),
        (PlayerConditions.Poisoned,    Base.SystemText.Condition_Poisoned),
        (PlayerConditions.Ill,         Base.SystemText.Condition_Ill),
        (PlayerConditions.Exhausted,   Base.SystemText.Condition_Exhausted),
        (PlayerConditions.Paralysed,   Base.SystemText.Condition_Paralyzed),
        (PlayerConditions.Fleeing,     Base.SystemText.Condition_Fleeing),
        (PlayerConditions.Intoxicated, Base.SystemText.Condition_Intoxicated),
        (PlayerConditions.Blind,       Base.SystemText.Condition_Blind),
        (PlayerConditions.Panicking,   Base.SystemText.Condition_Panicking),
        (PlayerConditions.Asleep,      Base.SystemText.Condition_Asleep),
        (PlayerConditions.Insane,      Base.SystemText.Condition_Insane),
        (PlayerConditions.Irritated,   Base.SystemText.Condition_Irritated),
    ];

    readonly PartyMemberId _activeCharacter;

    public ConditionsListPanel(PartyMemberId activeCharacter)
    {
        _activeCharacter = activeCharacter;
        On<SheetChangedEvent>(e =>
        {
            if (e.Id == _activeCharacter.ToSheet())
                Rebuild();
        });
    }

    protected override void Subscribed() => Rebuild();

    void Rebuild()
    {
        RemoveAllChildren();
        var player = TryResolve<IParty>()?[_activeCharacter];
        var conditions = player?.Effective.Combat.Conditions ?? PlayerConditions.None;

        var active = new List<IUiElement>();
        foreach (var (flag, textId) in _conditionMap)
        {
            if ((conditions & flag) != 0)
                active.Add(new Label(textId));
        }

        if (active.Count > 0)
            AttachChild(new VerticalStacker(active));
    }
}