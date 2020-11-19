﻿using System;

namespace Konata.Model.Package.Tlv.TlvModel
{
    public class T536Body : TlvBody
    {
        public readonly byte[] _loginExtraData;

        public T536Body(byte[] loginExtraData, int loginExtraDataLength)
            : base()
        {
            _loginExtraData = loginExtraData;

            PutBytes(_loginExtraData);
        }

        public T536Body(byte[] data)
            : base(data)
        {
            TakeBytes(out _loginExtraData, Prefix.None);
        }
    }
}
