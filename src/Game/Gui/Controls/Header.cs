using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls;

public class Header : UiElement // Header with midlines on either side
{
    readonly InkId _inkId;
    readonly StringId _id;
    readonly int _padding;

    public Header(TextId id, int padding = 0) : this(id, padding, Base.Ink.White) { } 
    public Header(TextId id, int padding, InkId color)
    {
        _id = id;
        _padding = padding;
        _inkId = color;
    }

    public Header(StringId id, int padding = 0) : this(id, padding, Base.Ink.White) { }
    public Header(StringId id, int padding, InkId color)
    {
        _id = id;
        _padding = padding;
        _inkId = color;
    }

    // Note: When using this constructor with a color other than white the IText needs to have its Ink set explicitly.
    public Header(IText source, int padding = 0) : this(source, padding, Base.Ink.White) { }
    public Header(IText source, int padding, InkId color)
    {
        _padding = padding;
        _inkId = color;
        Build(source);
    }

    void Build(IText source)
    {
        var text = new MinimumSize(new UiText(source));
        var elements = new List<IUiElement>();
        if (_padding > 0)
            elements.Add(new Spacing(_padding, 0));

        elements.Add(new MidLine(_inkId));
        elements.Add(text);
        elements.Add(new MidLine(_inkId));

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
        if (_inkId != Base.Ink.White)
            tf.Ink(_inkId);

        var text = tf.Format(_id);
        Build(text);
    }
}