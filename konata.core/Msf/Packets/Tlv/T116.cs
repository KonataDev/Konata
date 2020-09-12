﻿using Konata.Utils;

namespace Konata.Msf.Packets.Tlvs
{
    public class T116 : TlvBase
    {
        private const sbyte _ver = 0;

        private readonly int _bitmap;
        private readonly int _getSig;
        private readonly long[] _subAppIdList;

        public T116(int bitmap, int getSig, long[] subAppIdList = null)
        {
            if (subAppIdList == null)
                _subAppIdList = new long[] { 1600000226L };

            _bitmap = bitmap;
            _getSig = getSig;
        }

        public override ushort GetTlvCmd()
        {
            return 0x116;
        }

        public override byte[] GetTlvBody()
        {
            StreamBuilder builder = new StreamBuilder();
            builder.PutInt8(_ver);
            builder.PutUintBE(_bitmap);
            builder.PutUintBE(_getSig);
            builder.PutInt8((sbyte)_subAppIdList.Length);
            foreach (long element in _subAppIdList)
            {
                builder.PutUintBE((int)element);
            }
            return builder.GetBytes();
        }
    }
}