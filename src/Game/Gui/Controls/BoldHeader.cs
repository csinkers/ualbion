using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls;

class BoldHeader : UiElement
{
    readonly StringId _id;

    public BoldHeader(TextId id) : this(new StringId(id)) { }
    public BoldHeader(StringId id) => _id = id;
    public BoldHeader(IText source) => AttachChild(new UiText(source));

    protected override void Subscribed()
    {
        if (Children.Count != 0)
            return;

        var tf = Resolve<ITextFormatter>();
        var text = tf.NoWrap().Fat().Center().Format(_id);
        AttachChild(new UiText(text));
    }
}