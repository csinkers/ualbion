namespace UAlbion.Formats.Assets
{
    public class ItemProxy : IItem
    {
        public ItemProxy(ItemId id) => Id = id;
        public ItemId Id { get; }

        public bool Equals(IContents other) => Equals((object)other);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return ((ItemProxy)obj).Id == Id;
        }

        public override string ToString() => $"Proxy for {Id}";
        public override int GetHashCode() => (int) Id;
    }
}
