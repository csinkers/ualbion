using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats
{
    public class EventFormatter
    {
        readonly Func<StringId, string> _stringLoadFunc;
        readonly AssetId _textSourceId;

        public EventFormatter(Func<StringId, string> stringLoadFunc, AssetId textSourceId)
        {
            _stringLoadFunc = stringLoadFunc;
            _textSourceId = textSourceId;
        }

        public string Format(IEventNode e, int idOffset = 0)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var nodeText = e.ToString(idOffset);
            if (e.Event is TextEvent textEvent)
            {
                var text = _stringLoadFunc(new StringId(_textSourceId, textEvent.SubId));
                return $"{nodeText} // \"{text}\"";
            }

            return nodeText;
        }

        public string FormatChain(EventChain chain)
        {
            if (chain == null) 
                return null;
            var sb = new StringBuilder();

            var uniqueEvents = new HashSet<IEventNode>();
            void Visit(IEventNode e)
            {
                while (true)
                {
                    if (e == null)
                        return;

                    if (!uniqueEvents.Add(e)) 
                        break;

                    if (e is IBranchNode branch)
                        Visit(branch.NextIfFalse);
                    e = e.Next;
                }
            }

            Visit(chain.FirstEvent);
            var sorted = uniqueEvents.OrderBy(x => x.Id).ToList();
            foreach (var e in sorted)
                sb.AppendLine(Format(e, sorted[0].Id));

            return sb.ToString();
        }
    }
}
