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

        public string Format(IEventNode e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var nodeText = e.ToString();
            if (e.Event is TextEvent textEvent)
            {
                var text = _stringLoadFunc(new StringId(_textSourceId, textEvent.TextId));
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

                    if (e is IBranchNode branch && uniqueEvents.Add(branch))
                        Visit(branch.NextIfFalse);
                    e = e.Next;
                }
            }

            Visit(chain.FirstEvent);
            foreach (var e in uniqueEvents.OrderBy(x => x.Id))
                sb.AppendLine(Format(e));

            return sb.ToString();
        }
    }
}
