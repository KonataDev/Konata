﻿using System;
using System.Text;

using Konata.Core.Event;
using Konata.Core.Packet;
using Konata.Core.Packet.Oidb.OidbModel;

namespace Konata.Core.Service.OidbSvc
{
    [SSOService("OidbSvc.0x8a0_1", "Kick member in the group")]
    [Event(typeof(GroupKickMemberEvent))]
    class Oidb0x8a0_1 : ISSOService
    {
        public bool Parse(SSOFrame input, SignInfo signInfo, out ProtocolEvent output)
        {
            throw new NotImplementedException();
        }

        public bool Build(Sequence sequence, GroupKickMemberEvent input, SignInfo signInfo,
            out int newSequence, out byte[] output)
        {
            output = null;
            newSequence = sequence.NewSequence;

            var oidbRequest = new OidbCmd0x8a0_1(input.GroupUin, input.MemberUin, input.ToggleType);

            if (SSOFrame.Create("OidbSvc.0x8a0_1", PacketType.TypeB,
            newSequence, sequence.Session, oidbRequest, out var ssoFrame))
            {
                if (ServiceMessage.Create(ssoFrame, AuthFlag.D2Authentication,
                signInfo.UinInfo.Uin, signInfo.D2Token, signInfo.D2Key, out var toService))
                {
                    return ServiceMessage.Build(toService, out output);
                }
            }

            return false;
        }

        public bool Build(Sequence sequence, ProtocolEvent input, SignInfo signInfo,
            out int newSequence, out byte[] output)
            => Build(sequence, (GroupKickMemberEvent)input, signInfo, out newSequence, out output);

    }
}
