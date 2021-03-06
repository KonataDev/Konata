﻿using System;

using Konata.Core.Event;
using Konata.Core.Event.EventModel;
using Konata.Core.Packet;
using Konata.Core.Packet.SvcRequest;

namespace Konata.Core.Service.Friendlist
{
    [Service("friendlist.ModifyGroupCardReq", "Modify group card")]
    [EventDepends(typeof(GroupModifyMemberCardEvent))]
    public class ModifyGroupCardReq : IService
    {
        public bool Parse(SSOFrame input, SignInfo signInfo, out ProtocolEvent output)
        {
            throw new NotImplementedException();
        }

        public bool Build(Sequence sequence, GroupModifyMemberCardEvent input,
            SignInfo signInfo, BotDevice device, out int newSequence, out byte[] output)
        {
            output = null;
            newSequence = sequence.NewSequence;

            var svcRequest = new SvcReqModifyGroupCard(input.GroupUin, input.MemberUin, input.MemberCard);

            if (SSOFrame.Create("friendlist.ModifyGroupCardReq", PacketType.TypeB,
               newSequence, sequence.Session, svcRequest, out var ssoFrame))
            {
                if (ServiceMessage.Create(ssoFrame, AuthFlag.D2Authentication,
                    signInfo.Account.Uin, signInfo.Session.D2Token, signInfo.Session.D2Key, out var toService))
                {
                    return ServiceMessage.Build(toService, device, out output);
                }
            }

            return false;
        }

        public bool Build(Sequence sequence, ProtocolEvent input,
            SignInfo signInfo, BotDevice device, out int newSequence, out byte[] output)
            => Build(sequence, (GroupModifyMemberCardEvent)input, signInfo, device, out newSequence, out output);
    }
}
