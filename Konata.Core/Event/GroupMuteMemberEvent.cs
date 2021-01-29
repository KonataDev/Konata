using System;

namespace Konata.Core.Event
{
    public class GroupMuteMemberEvent : BaseEvent
    {
        public uint GroupUin { get; set; }

        public uint MemberUin { get; set; }

        /// <summary>
        /// <b>MuteMember</b>: Mute time <br/>
        /// </summary>
        public uint? TimeSeconds { get; set; }
    }
}
