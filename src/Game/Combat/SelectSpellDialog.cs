using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Combat;

public class SelectSpellDialog : ModalDialog
{
    const int MaxVisibleSpells = 10;
    const int StrengthBarSegments = 10;

    readonly List<KeyValuePair<SpellId, ushort>> _spellList;
    readonly Action<SpellId> _onSelected;

    public SelectSpellDialog(IEnumerable<KeyValuePair<SpellId, ushort>> spells, Action<SpellId> onSelected, int depth = 0)
        : base(DialogPositioning.Center, depth)
    {
        _spellList = spells.ToList();
        _onSelected = onSelected;
    }

    protected override void Subscribed()
    {
        var rows = new List<Button>();
        for (int i = 0; i < Math.Min(_spellList.Count, MaxVisibleSpells); i++)
            rows.Add(BuildSpellRow(_spellList[i].Key, _spellList[i].Value, _onSelected));
        AttachChild(new DialogFrame(new VerticalStacker(rows.ToArray()) { Greedy = false }));
    }

    Button BuildSpellRow(SpellId spellId, ushort strength, Action<SpellId> onSelected)
    {
        var spellData = Assets.LoadSpell(spellId);
        var spellName = spellData != null ? Assets.LoadStringSafe(spellData.Name) : "?";
        byte spCost = spellData?.Cost ?? 0;
        int segments = Math.Clamp((int)((strength + 9) / 10), 1, StrengthBarSegments);

        var barSegments = new List<IUiElement>();
        for (int i = 0; i < StrengthBarSegments; i++)
        {
            barSegments.Add(new UiRectangle(i < segments ? CommonColor.Green6 : CommonColor.Grey6)
            {
                DrawSize = new Vector2(8, 10)
            });
        }

        var tf = Resolve<ITextFormatter>();
        var rowContent = new HorizontalStacker(
            new Spacing(4, 0),
            new UiText(new LiteralText(spellName)),
            new Spacing(8, 0),
            new HorizontalStacker(barSegments),
            new Spacing(8, 0),
            new UiText(new LiteralText(spCost.ToString())),
            new Spacing(4, 0)
        );

        return new Button(rowContent).OnClick(() =>
        {
            onSelected(spellId);
            Remove();
        });
    }
}
