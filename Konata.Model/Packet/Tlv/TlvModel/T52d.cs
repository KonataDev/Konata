﻿using System;
using Konata.Model.Packet.Protobuf;

namespace Konata.Model.Packet.Tlv.TlvModel
{
    public class T52dBody : TlvBody
    {
        public readonly DeviceReport _deviceReportInfo;

        public T52dBody(string bootLoader, string version, string codeName,
            string incremental, string fingerprint, string bootId, string androidId,
            string baseBand, string innerVersion)
            : base()
        {
            var report = new DeviceReport(bootLoader, version, codeName,
                incremental, fingerprint, bootId, androidId, baseBand, innerVersion);

            PutProtoNode(report);
        }

        public T52dBody(DeviceReport report)
        {
            PutProtoNode(report);
        }

        public T52dBody(byte[] data)
            : base(data)
        {
            EatBytes(RemainLength);
        }
    }
}
