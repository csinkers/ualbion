using System.Collections.Generic;

namespace UAlbion.Core.Entities
{
    class Conversation
    {
        readonly IList<Animate> _participants = new List<Animate>();
        readonly IDictionary<string, string> _keywords = new Dictionary<string, string>();

    }
}
