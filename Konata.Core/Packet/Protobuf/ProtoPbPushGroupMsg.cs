﻿using System;
using Konata.Utils.Protobuf;

namespace Konata.Core.Packet.Protobuf
{
    public class ProtoPbPushGroupMsg : ProtoTreeRoot
    {
        public uint GroupUin { get; private set; }

        public string GroupName { get; private set; }

        public uint MemberUin { get; private set; }
        public string MemberName { get; private set; }

        public string MsgContent { get; private set; }

        public ProtoPbPushGroupMsg(byte[] payload)
            : base(payload, true)
        {
            GroupName = GetLeafString("0A.0A.4A.42");
            GroupUin = (uint)GetLeafVar("0A.0A.4A.08");

            MemberName = GetLeafString("0A.0A.4A.22");
            MemberUin = (uint)GetLeafVar("0A.0A.08");

            MsgContent = GetLeafString("0A.1A.0A.12.0A.0A");
        }
    }
}
