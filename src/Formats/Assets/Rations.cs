using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class Rations : IContents
{
    Rations() { }
    public static readonly Rations Instance = new();
    public bool Equals(IContents obj) => Equals((object)obj);
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return true;
    }

    public override int GetHashCode() => 72;
    public SpriteId Icon => Base.CoreGfx.UiFood;
    public int IconSubId => 0;
    public byte IconAnim => 0;
}