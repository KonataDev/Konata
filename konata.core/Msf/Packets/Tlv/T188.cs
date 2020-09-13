﻿using Konata.Utils;
using Konata.Msf.Utils.Crypt;

namespace Konata.Msf.Packets.Tlvs
{
    public class T188 : TlvBase
    {
        private readonly byte[] _androidId;

        public T188(byte[] androidId)
        {
            _androidId = androidId;
        }

        public T188(string androidId)
        {
            _androidId = Hex.HexStr2Bytes(androidId);
        }

        public override void PutTlvCmd()
        {
            return 0x188;
        }

        public override void PutTlvBody()
        {
            StreamBuilder builder = new StreamBuilder();
            builder.PutBytes(new Md5Cryptor().Encrypt(_androidId));
            return builder.GetBytes();
        }
    }
}
