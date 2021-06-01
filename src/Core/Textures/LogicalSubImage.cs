using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Core.Textures
{
    class LogicalSubImage
    {
        public LogicalSubImage(int id) { Id = id; }

        public int Id { get; }
        public int W { get; set; }
        public int H { get; set; }
        public int Frames { get; set; }
        public bool IsAlphaTested { get; set; }
        public byte? TransparentColor { get; set; }
        public IList<SubImageComponent> Components { get; } = new List<SubImageComponent>();

        public override string ToString() => $"LSI{Id} {W}x{H}:{Frames} {string.Join("; ", Components.Select(x => x.ToString()))}";
    }
}