﻿using Konata.Utils;

namespace Konata.Msf.Packets.Tlvs
{
    public class T108 : TlvBase
    {
        private readonly byte[] _ksid;

        public T108(byte[] ksid)
        {
            _ksid = ksid;
        }

        public override ushort GetTlvCmd()
        {
            return 0x108;
        }

        public override byte[] GetTlvBody()
        {
            StreamBuilder builder = new StreamBuilder();
            builder.PutBytes(_ksid);
            return builder.GetBytes();
        }
    }
}
