using System;

namespace Konata.Core.Event.EventModel
{
    public class GroupMessageEvent : ProtocolEvent
    {
        public string GroupName { get; set; }

        public uint GroupUin { get; set; }

        public uint MemberUin { get; set; }

        public string MemberCard { get; set; }

        public string MessageContent { get; set; }
    }
}
