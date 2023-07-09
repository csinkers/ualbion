using System;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public struct StringId : IEquatable<StringId>
{
    public StringId(TextId id) { Id = id; SubId = 0; }
    public StringId(StringSetId id, ushort subId) { Id = id; SubId = subId; }
    public override string ToString() => $"{Id}:{SubId}";
    public AssetId Id { get; }
    public ushort SubId { get; }

    public override bool Equals(object obj) => obj is StringId other && Equals(other);
    public bool Equals(StringId other) => Id == other.Id && SubId == other.SubId;
    public static bool operator ==(StringId left, StringId right) => left.Equals(right);
    public static bool operator !=(StringId left, StringId right) => !(left == right);
    public override int GetHashCode() => 17 * Id.ToInt32() ^ SubId;

    public static StringId Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentNullException(nameof(s));

        int index = s.IndexOf(':', StringComparison.Ordinal);

        if (index == -1)
        {
            var id = TextId.Parse(s);
            return new StringId(id);
        }
        else
        {
            var id = StringSetId.Parse(s[..index]);
            var subId = ushort.Parse(s[(index + 1)..]);
            return new StringId(id, subId);
        }
    }
}