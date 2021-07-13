using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text
{
    public class SimpleText : UiElement, IText, ITextBuilder<SimpleText>
    {
        readonly TextBlock _block = new();
        public SimpleText(string literal) => _block.Text = literal;
        public int Version { get; private set; }
        public IEnumerable<TextBlock> GetBlocks() => new[] { _block };
        public string Text
        {
            get => _block.Text;
            set
            {
                _block.Text = value;
                Version++;
            }
        }

        protected override void Subscribed()
        {
            if (Children.Count != 0)
                return;

            AttachChild(new UiText(this));
        }

        public SimpleText Fat() { _block.Style = TextStyle.Fat; Version++; return this; }
        public SimpleText Left() { _block.Alignment = TextAlignment.Left; Version++; return this; }
        public SimpleText Center() { _block.Alignment = TextAlignment.Center; Version++; return this; }
        public SimpleText Right() { _block.Alignment = TextAlignment.Right; Version++; return this; }
        public SimpleText Justify() { _block.Alignment = TextAlignment.Justified; Version++; return this; }
        public SimpleText NoWrap() { _block.ArrangementFlags |= TextArrangementFlags.NoWrap; Version++; return this; }
        public SimpleText Ink(FontColor color) { _block.Color = color; Version++; return this; }
        public SimpleText Language(string language) => this;
        public override string ToString() => $"SimpleText \"{_block.Text}\"";
    }
}
