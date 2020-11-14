﻿using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using Konata.Msf.Crypto;
using Konata.Msf.Packets.Oicq;
using Konata.Msf.Packets.Protobuf;
using Konata.Events;

namespace Konata.Msf
{
    public class UserSigInfo : EventComponent
    {
        public uint Uin { get; private set; }

        public string UinName { get; set; }

        public byte[] PasswordMd5 { get; private set; }

        public byte[] SyncCookie { get; set; }

        #region WtLogin

        public byte[] GSecret { get; private set; }

        public string DPassword { get; private set; }

        public string WtLoginSmsToken { get; set; }

        public string WtLoginSmsPhone { get; set; }

        public string WtLoginSession { get; set; }

        public OicqStatus WtLoginStatus { get; set; }

        #endregion

        #region Keys And Tokens

        public byte[] Tlv106Key { get; private set; }

        public byte[] TgtgKey { get; private set; } =
        {
            0x2E, 0x39, 0x9A, 0x9C, 0xF2, 0x57, 0x12, 0xF8,
            0x1E, 0x5B, 0x63, 0x2E, 0xB3, 0xB3, 0xF7, 0x9F
        };

        public byte[] RandKey { get; private set; } =
        {
            0xE2, 0xED, 0x53, 0x77, 0xAD, 0xFD, 0x99, 0x83,
            0x56, 0xEB, 0x8B, 0x4C, 0x62, 0x7C, 0x22, 0xC4
        };

        public byte[] ShareKey { get; private set; } =
        {
            0x4D, 0xA0, 0xF6, 0x14, 0xFC, 0x9F, 0x29, 0xC2,
            0x05, 0x4C, 0x77, 0x04, 0x8A, 0x65, 0x66, 0xD7
        };

        public byte[] ZeroKey { get; private set; } =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public byte[] DefaultPublicKey { get; private set; } =
        {
            0x02, 0x0B, 0x03, 0xCF, 0x3D, 0x99, 0x54, 0x1F,
            0x29, 0xFF, 0xEC, 0x28, 0x1B, 0xEB, 0xBD, 0x4E,
            0xA2, 0x11, 0x29, 0x2A, 0xC1, 0xF5, 0x3D, 0x71,
            0x28
        };

        public byte[] ServerPublicKey { get; private set; } =
        {
            0x04, 0x92, 0x8D, 0x88, 0x50, 0x67, 0x30, 0x88,
            0xB3, 0x43, 0x26, 0x4E, 0x0C, 0x6B, 0xAC, 0xB8,
            0x49, 0x6D, 0x69, 0x77, 0x99, 0xF3, 0x72, 0x11,
            0xDE, 0xB2, 0x5B, 0xB7, 0x39, 0x06, 0xCB, 0x08,
            0x9F, 0xEA, 0x96, 0x39, 0xB4, 0xE0, 0x26, 0x04,
            0x98, 0xB5, 0x1A, 0x99, 0x2D, 0x50, 0x81, 0x3D,
            0xA8
        };

        public byte[] TgtKey { get; set; }

        public byte[] TgtToken { get; set; }

        public byte[] D2Key { get; set; }

        public byte[] D2Token { get; set; }

        public byte[] GtKey { get; set; }

        public byte[] StKey { get; set; }

        public byte[] WtSessionTicketSig { get; set; }

        public byte[] WtSessionTicketKey { get; set; }

        #endregion

        public UserSigInfo(EventPumper eventPumper, uint uin, string password)
            : base(eventPumper)
        {
            eventHandlers += OnUpdateSigInfo;

            Uin = uin;

            PasswordMd5 = new Md5Cryptor().Encrypt(Encoding.UTF8.GetBytes(password));
            Tlv106Key = new Md5Cryptor().Encrypt(PasswordMd5
                        .Concat(new byte[] { 0x00, 0x00, 0x00, 0x00 })
                        .Concat(BitConverter.GetBytes(uin).Reverse()).ToArray());

            DPassword = MakeDpassword();
            GSecret = MakeGSecret(DeviceInfo.System.Imei, DPassword, null);

            SyncCookie = MakeSyncCookie();
        }

        private EventParacel OnUpdateChallengeInfo(EventParacel eventParacel)
        {

        }

        private EventParacel OnUpdateSigInfo(EventParacel eventParacel)
        {
            if (eventParacel is EventUpdateSigInfo info)
            {
                D2Key = info.D2Key ?? D2Key;
                D2Token = info.D2Token ?? D2Token;

                TgtKey = info.TgtKey ?? TgtKey;
                TgtToken = info.TgtToken ?? TgtToken;

                GtKey = info.GtKey ?? GtKey;
                StKey = info.StKey ?? StKey;

                WtSessionTicketSig = info.WtSessionTicketSig ?? WtSessionTicketSig;
                WtSessionTicketKey = info.WtSessionTicketKey ?? WtSessionTicketKey;

                return EventParacel.Accept;
            }

            return EventParacel.Reject;
        }

        private EventParacel OnUpdateSyncCookie(EventParacel eventParacel)
        {
            if (eventParacel is EventUpdateSigInfo info)
            {

                return EventParacel.Accept;
            }
            return EventParacel.Reject;
        }

        private static byte[] MakeGSecret(string imei, string dpwd, byte[] salt)
        {
            byte[] buffer;
            var imeiByte = Encoding.UTF8.GetBytes(imei);
            var dpwdByte = Encoding.UTF8.GetBytes(dpwd);

            if (salt != null)
            {
                buffer = new byte[imeiByte.Length + dpwdByte.Length + salt.Length];

                Array.Copy(imeiByte, buffer, imei.Length);
                Array.Copy(dpwdByte, 0, buffer, imei.Length, dpwdByte.Length);
                Array.Copy(salt, 0, buffer, imei.Length + dpwdByte.Length, salt.Length);
            }
            else
            {
                buffer = new byte[imeiByte.Length + dpwdByte.Length];

                Array.Copy(imeiByte, buffer, imei.Length);
                Array.Copy(dpwdByte, 0, buffer, imei.Length, dpwdByte.Length);
            }

            return new Md5Cryptor().Encrypt(buffer);
        }

        private static string MakeDpassword()
        {
            try
            {
                var random = new Random();
                var seedTable = new byte[16];

                bool RandBoolean()
                {
                    return random.Next(0, 1) == 1;
                }

                using (RNGCryptoServiceProvider SecurityRandom =
                    new RNGCryptoServiceProvider())
                {
                    SecurityRandom.GetBytes(seedTable);
                }

                for (int i = 0; i < seedTable.Length; ++i)
                {
                    seedTable[i] = (byte)(Math.Abs(seedTable[i] % 26) + (RandBoolean() ? 97 : 65));
                }

                return Encoding.UTF8.GetString(seedTable);
            }
            catch
            {
                return "1234567890123456";
            }
        }

        private byte[] MakeSyncCookie()
        {
            return new SyncCookie(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).Serialize().GetBytes();
        }

        public class EventUpdateSyncCookie : EventParacel
        {
            public byte[] SyncCookie { get; set; }
        }

        public class EventUpdateChallengeInfo : EventParacel
        {
            public string Session { get; set; }

            public string SmsPhone { get; set; }

            public string SmsToken { get; set; }
        }

        public class EventUpdateSigInfo : EventParacel
        {
            public byte[] TgtKey { get; set; }

            public byte[] TgtToken { get; set; }

            public byte[] D2Key { get; set; }

            public byte[] D2Token { get; set; }

            public byte[] GtKey { get; set; }

            public byte[] StKey { get; set; }

            public byte[] WtSessionTicketSig { get; set; }

            public byte[] WtSessionTicketKey { get; set; }

            public class Info
            {
                public uint Age { get; set; }

                public uint Face { get; set; }

                public string Name { get; set; }
            }

            public Info UinInfo { get; set; }
        }
    }
}
