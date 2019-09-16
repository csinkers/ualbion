using System.Collections.Generic;

namespace UAlbion.Game.Entities
{
    class Conversation
    {
        readonly IList<int> _participants = new List<int>();
        readonly IDictionary<string, string> _keywords = new Dictionary<string, string>();
    }
}
