﻿using System;

namespace Konata.Model.Packet.Tlv.TlvModel
{
    public class T174Body : TlvBody
    {
        public readonly string _smsToken;

        public T174Body(string smsToken)
            : base()
        {
            _smsToken = smsToken;

            PutString(_smsToken);
        }

        public T174Body(byte[] data)
            : base(data)
        {
            TakeString(out _smsToken, Prefix.None);
        }
    }
}
