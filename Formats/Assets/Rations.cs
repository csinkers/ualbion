namespace UAlbion.Formats.Assets
{
    public class Rations : IContents
    {
        public bool Equals(IContents obj) => Equals((object)obj);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return true;
        }

        public override int GetHashCode() => 72;
    }
}