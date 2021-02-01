using System;

namespace Konata.Core.Event
{
    public class PacketEvent : BaseEvent
    {
        public enum Type
        {
            Send,
            Receive
        }

        public Type EventType { get; set; }

        public byte[] Buffer { get; set; }
    }
}
