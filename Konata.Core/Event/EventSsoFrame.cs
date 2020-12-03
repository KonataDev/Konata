﻿using System;
using System.Collections.Generic;

using Konata.Utils.IO;
using Konata.Core.Packet;
using Konata.Runtime.Base.Event;

namespace Konata.Core.Event
{
    public enum PacketType : uint
    {
        TypeA = 0x0A,
        TypeB = 0x0B
    }

    public class EventSsoFrame : KonataEventArgs
    {
        private uint _session;
        private uint _sequence;
        private string _command;
        private ByteBuffer _payload;
        private PacketType _packetType;
        private byte[] _tgtoken;

        public uint Session { get => _session; }

        public string Command { get => _command; }

        public uint Sequence { get => _sequence; }

        public byte[] Tgtoken { get => _tgtoken; }

        public ByteBuffer Payload { get => _payload; }

        public PacketType PacketType { get => _packetType; }

        public static bool Parse(EventServiceMessage fromService, out EventSsoFrame output)
        {
            output = new EventSsoFrame
            {
                _packetType = fromService.MessagePktType
            };

            var read = new ByteBuffer(fromService.FrameBytes);
            {
                read.TakeUintBE(out var length);
                {
                    if (length > read.Length)
                        return false;
                }

                read.TakeUintBE(out output._sequence);

                read.TakeUintBE(out var zeroUint);
                {
                    if (zeroUint != 0)
                        return false;
                }

                read.TakeBytes(out var unknownBytes,
                        ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                read.TakeString(out output._command,
                    ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                read.TakeBytes(out var session,
                    ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);
                {
                    if (session.Length != 4)
                        return false;

                    output._session = ByteConverter.BytesToUInt32(session, 0);
                }

                read.TakeUintBE(out zeroUint);
                {
                    if (zeroUint != 0)
                        return false;
                }

                read.TakeBytes(out var bytes,
                    ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);
                {
                    output._payload = new ByteBuffer(bytes);
                }
            }

            output.Owner = fromService.Owner;
            return true;
        }

        public static ByteBuffer Build(EventSsoFrame ssoFrame)
        {
            byte[] unknownBytes0 = { };
            byte[] unknownBytes1 = { };
            string unknownString = $"||A{AppInfo.apkVersionName}.{AppInfo.appRevision}";
            byte[] sessionBytes = ByteConverter.UInt32ToBytes(ssoFrame._session, Endian.Big);

            var write = new PacketBase();
            {
                write.EnterBarrier(ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix, Endian.Big);
                {
                    if (ssoFrame.PacketType == PacketType.TypeA)
                    {
                        write.PutUintBE(ssoFrame._sequence);
                        write.PutUintBE(AppInfo.subAppId);
                        write.PutUintBE(AppInfo.subAppId);
                        write.PutHexString("01 00 00 00 00 00 00 00 00 00 01 00");

                        write.PutBytes(ssoFrame._tgtoken ?? new byte[0],
                            ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutString(ssoFrame._command,
                            ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutBytes(sessionBytes,
                            ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutString(DeviceInfo.System.Imei,
                            ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutBytes(unknownBytes0,
                            ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutString(unknownString,
                            ByteBuffer.Prefix.Uint16 | ByteBuffer.Prefix.WithPrefix);

                        write.PutBytes(unknownBytes1,
                            ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);
                    }
                    else if (ssoFrame.PacketType == PacketType.TypeB)
                    {
                        write.PutString(ssoFrame._command,
                           ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutBytes(sessionBytes,
                           ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);

                        write.PutBytes(unknownBytes0,
                           ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);
                    }
                }
                write.LeaveBarrier();

                write.PutByteBuffer(ssoFrame.Payload,
                    ByteBuffer.Prefix.Uint32 | ByteBuffer.Prefix.WithPrefix);
            }

            return write;
        }

        public static bool Create(string command, PacketType pktType, uint sequence,
            byte[] tgtoken, uint session, ByteBuffer payload, out EventSsoFrame ssoFrame)
        {
            ssoFrame = new EventSsoFrame
            {
                _command = command,
                _sequence = sequence,
                _session = session,
                _packetType = pktType,
                _payload = payload,
                _tgtoken = tgtoken
            };

            return true;
        }

        public static bool Create(string command, PacketType pktType, uint sequence,
            uint session, ByteBuffer payload, out EventSsoFrame ssoFrame)
          => Create(command, pktType, sequence, null, session, payload, out ssoFrame);
    }
}