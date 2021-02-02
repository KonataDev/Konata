using System;

namespace Konata.Core
{
    public class BotConfig
    {
        public uint Uin { get; set; }

        public string Password { get; set; }

        public bool UseIPv6Connection { get; set; } = false;

        public bool ReConnectWhileLinkDown { get; set; } = true;

        public uint ReConnectTryCount { get; set; } = 3;
    }
}
