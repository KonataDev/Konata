using System;
using System.Text;

namespace Konata.Core
{
    public class BotConfig
    {
        public uint BotUin;
        public string BotPassword;

        public bool ReConnectWhileLinkDown = true;
        public uint ReConnectTryCount = 3;

    }
}
