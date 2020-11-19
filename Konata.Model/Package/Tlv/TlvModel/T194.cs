﻿using System.Text;
using Konata.Utils.Crypto;

namespace Konata.Model.Package.Tlv.TlvModel
{
    public class T194Body : TlvBody
    {
        public readonly byte[] _imsiMd5;

        public T194Body(string imsi)
            : base()
        {
            _imsiMd5 = new Md5Cryptor().Encrypt(Encoding.UTF8.GetBytes(imsi));

            PutBytes(_imsiMd5);
        }

        public T194Body(byte[] data)
            : base(data)
        {
            TakeBytes(out _imsiMd5, Prefix.None);
        }
    }
}
