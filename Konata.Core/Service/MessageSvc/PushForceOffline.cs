﻿using System;

using Konata.Core.Event;
using Konata.Core.Packet;

namespace Konata.Core.Service.MessageSvc
{
    [SSOService("MessageSvc.PushForceOffline", "Force offline")]
    public class PushForceOffline : ISSOService
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
