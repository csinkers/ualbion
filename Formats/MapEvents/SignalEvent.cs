using System.Diagnostics;
using System.IO;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SignalEvent : IMapEvent
    {
        public static SignalEvent Translate(SignalEvent e, ISerializer s)
        {
            e ??= new SignalEvent();
            s.Dynamic(e, nameof(SignalId));
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk2 == 0);
            Debug.Assert(e.Unk3 == 0);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk6 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public byte SignalId { get; private set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk6 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"signal {SignalId}";
        public MapEventType EventType => MapEventType.Signal;
    }
}
