using System;

using Konata.Core.Packet;
using Konata.Core.Event;

namespace Konata.Core.Service.MessageSvc
{
    [Service("MessageSvc.PushNotify", "Push notify on received a private message")]
    public class PushNotify : IService
    {
        public bool Parse(SSOFrame input, SignInfo signInfo, out ProtocolEvent output)
        {
            throw new NotImplementedException();
        }

        public bool Build(Sequence sequence, ProtocolEvent input, SignInfo signInfo,
            out int newSequence, out byte[] output)
        {
            throw new NotImplementedException();
        }
    }
}
