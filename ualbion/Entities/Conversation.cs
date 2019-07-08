using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAlbion.Entities
{
    class Conversation
    {
        readonly IList<Animate> _participants = new List<Animate>();
        readonly IDictionary<string, string> _keywords = new Dictionary<string, string>();

    }
}
