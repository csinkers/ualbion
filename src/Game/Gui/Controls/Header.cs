using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls;

public class Header : UiElement // Header with midlines on either side
{
    readonly FontColor _color;
    readonly StringId _id;
    readonly int _padding;

    public Header(TextId id, int padding = 0, FontColor color = FontColor.White)
    {
        _id = id;
        _padding = padding;
        _color = color;
    }

    public Header(StringId id, int padding = 0, FontColor color = FontColor.White)
    {
        _id = id;
        _padding = padding;
        _color = color;
    }

    // Note: When using this constructor with a color other than white the IText needs to have its Ink set explicitly.
    public Header(IText source, int padding = 0, FontColor color = FontColor.White) 
    {
        _padding = padding;
        _color = color;
        Build(source);
    }

    void Build(IText source)
    {
        var text = new MinimumSize(new UiText(source));
        var lineColor = _color.GetLineColor();
        var elements = new List<IUiElement>();
        if (_padding > 0)
            elements.Add(new Spacing(_padding, 0));

        elements.Add(new MidLine(lineColor));
        elements.Add(text);
        elements.Add(new MidLine(lineColor));

        if (_padding > 0)
            elements.Add(new Spacing(_padding, 0));

        AttachChild(new HorizontalStack(elements));
    }

    protected override void Subscribed()
    {
        if (Children.Count != 0)
            return;

        var tf = Resolve<ITextFormatter>();
        tf.NoWrap().Center();
        if (_color != FontColor.White)
            tf.Ink(_color);

        var text = tf.Format(_id);
        Build(text);
    }
}