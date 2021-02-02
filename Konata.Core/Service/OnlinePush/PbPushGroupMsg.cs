using System;

using Konata.Core.Event;
using Konata.Core.Event.EventModel;
using Konata.Core.Packet;
using Konata.Utils.Protobuf;

namespace Konata.Core.Service.OnlinePush
{
    [Service("OnlinePush.PbPushGroupMsg", "Receive group message from server")]
    public class PbPushGroupMsg : IService
    {
        public bool Parse(SSOFrame input, SignInfo signInfo, out ProtocolEvent output)
        {
            var protoRoot = new ProtoTreeRoot(input.Payload.GetBytes());
            {
                // Simplest way to read group message.
                output = new GroupMessageEvent
                {
                    GroupUin = (uint)protoRoot.GetLeafVar("0A.0A.4A.08", out var _),
                    GroupName = protoRoot.GetLeafString("0A.0A.4A.42", out var _),

                    MemberUin = (uint)protoRoot.GetLeafVar("0A.0A.08", out var _),
                    MemberCard = protoRoot.GetLeafString("0A.0A.4A.22", out var _),

                    MessageContent = protoRoot.GetLeafString("0A.1A.0A.12.0A.0A", out var _),
                };
            }

            return true;
        }

        public bool Build(Sequence sequence, ProtocolEvent input, SignInfo signInfo,
            out int newSequence, out byte[] output) => throw new NotImplementedException();
    }
}
