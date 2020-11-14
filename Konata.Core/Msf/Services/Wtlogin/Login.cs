﻿using System;
using Konata.Utils;
using Konata.Events;
using Konata.Msf.Crypto;
using Konata.Msf.Packets;
using Konata.Msf.Packets.Tlv;
using Konata.Msf.Packets.Oicq;
using Konata.Msf.Packets.Sso;
using static Konata.Msf.UserSigInfo;

namespace Konata.Msf.Services.Wtlogin
{
    public class Login : Service
    {
        private Login()
        {
            Register("wtlogin.login", this);

            eventHandlers += OnHandleEvent;
        }

        public static Service Instance { get; } = new Login();

        public override bool OnRun(Core core, string method, params object[] args)
        {
            switch (method)
            {
                case "Request_TGTGT":
                    return Request_TGTGT(core);
                case "Request_SliderCaptcha":
                    return Request_SliderCaptcha(core, (string)args[0]);
                case "Request_SmsCaptcha":
                    return Request_SmsCaptcha(core, (string)args[0]);
                case "Request_RefreshSms":
                    return Request_RefreshSms(core);
                default: return false;
            }
        }

        public override bool OnHandle(Core core, params object[] args)
        {
            if (args == null || args.Length == 0)
                return false;

            var oicqRequest = new OicqRequest((byte[])args[0],
                core.SigInfo.ShareKey);

            Console.WriteLine($"  [oicqRequest] oicqCommand => {oicqRequest.oicqCommand}");
            Console.WriteLine($"  [oicqRequest] oicqVersion => {oicqRequest.oicqVersion}");
            Console.WriteLine($"  [oicqRequest] oicqStatus => {oicqRequest.oicqStatus}");

            core.SigInfo.WtLoginStatus = oicqRequest.oicqStatus;
            switch (oicqRequest.oicqStatus)
            {
                case OicqStatus.OK:
                    return Handle_WtloginSuccess(core, oicqRequest);

                case OicqStatus.DoVerifySliderCaptcha:
                    return Handle_VerifySliderCaptcha(core, oicqRequest);
                case OicqStatus.DoVerifyDeviceLockViaSms:
                    return Handle_VerifySmsCaptcha(core, oicqRequest);

                case OicqStatus.PreventByIncorrectUserOrPwd:
                    return Handle_InvalidUserOrPassword(core, oicqRequest);
                case OicqStatus.PreventByIncorrectSmsCode:
                    return Handle_InvalidSmsCode(core, oicqRequest);
                case OicqStatus.PreventByInvalidEnvironment:
                    return Handle_InvalidEnvironment(core, oicqRequest);
                case OicqStatus.PreventByLoginDenied:
                    return Handle_LoginDenied(core, oicqRequest);

                default: Handle_UnknownOicqRequest(core, oicqRequest); break;
            }

            return false;
        }

        public class EventWtLogin : EventParacel
        {

        }

        private EventParacel OnHandleEvent(EventParacel eventParacel)
        {
            switch (eventParacel)
            {
                default:
                    return EventParacel.Reject;
            }
        }

        #region Event Requests

        /// <summary>
        /// 請求 OicqRequestTgtgt
        /// </summary>
        /// <param name="core"></param>
        private bool Request_TGTGT(Core core)
        {
            Console.WriteLine("Submit OicqRequestTGTGT.");

            var ssoSeq = core.SsoMan.GetServiceSequence(name);
            var ssoSession = core.SsoMan.GetSsoSession();

            var ssoMessage = new SsoMessageTypeA(ssoSeq, name, ssoSession, null,
                new OicqRequestTgtgt(core.SigInfo.Uin, ssoSeq, core.SigInfo));

            return core.SsoMan.PostMessage(
                RequestFlag.WtLoginExchange, ssoMessage);
        }

        private EventParacel Request_Tgtgt()
        {
            Console.WriteLine("Submit OicqRequestTGTGT.");

        }

        /// <summary>
        /// 請求 OicqRequestCheckImage
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        private bool Request_SliderCaptcha(Core core, string ticket)
        {
            Console.WriteLine("Submit OicqRequestCheckImage.");

            var ssoSeq = core.SsoMan.GetServiceSequence(name);
            var ssoSession = core.SsoMan.GetSsoSession();

            var ssoMessage = new SsoMessageTypeA(ssoSeq, name, ssoSession, null,
                new OicqRequestCheckImage(core.SigInfo.Uin, core.SigInfo,
                    core.SigInfo.WtLoginSession, ticket));

            return core.SsoMan.PostMessage(
                RequestFlag.WtLoginExchange, ssoMessage);
        }

        /// <summary>
        /// 請求 OicqRequestCheckSms
        /// </summary>
        /// <param name="core"></param>
        /// <param name="smsCode"></param>
        /// <returns></returns>
        private bool Request_SmsCaptcha(Core core, string smsCode)
        {
            Console.WriteLine("Submit OicqRequestCheckSms.");

            var ssoSeq = core.SsoMan.GetServiceSequence(name);
            var ssoSession = core.SsoMan.GetSsoSession();

            var ssoMessage = new SsoMessageTypeA(ssoSeq, name, ssoSession, null,
                new OicqRequestCheckSms(core.SigInfo.Uin, core.SigInfo,
                    core.SigInfo.WtLoginSession, core.SigInfo.GSecret,
                    core.SigInfo.WtLoginSmsToken, smsCode));

            return core.SsoMan.PostMessage(
                RequestFlag.WtLoginExchange, ssoMessage);
        }

        /// <summary>
        /// 刷新SMS驗證碼. CD 60s
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        private bool Request_RefreshSms(Core core)
        {
            Console.WriteLine("Request send SMS.");

            var ssoSeq = core.SsoMan.GetServiceSequence(name);
            var ssoSession = core.SsoMan.GetSsoSession();

            var ssoMessage = new SsoMessageTypeA(ssoSeq, name, ssoSession, null,
                new OicqRequestRefreshSms(core.SigInfo.Uin, core.SigInfo,
                    core.SigInfo.WtLoginSession, core.SigInfo.WtLoginSmsToken));

            return core.SsoMan.PostMessage(
                RequestFlag.WtLoginExchange, ssoMessage);
        }

        #endregion

        #region Event Handlers

        private bool Handle_VerifySliderCaptcha(Core core, OicqRequest request)
        {
            Console.WriteLine("Do slider verification.");

            var tlvs = request.oicqRequestBody.TakeAllBytes(out var _);
            var unpacker = new TlvUnpacker(tlvs, true);

            Tlv tlv104 = unpacker.TryGetTlv(0x104);
            Tlv tlv192 = unpacker.TryGetTlv(0x192);
            if (tlv104 != null && tlv192 != null)
            {
                var sigSession = ((T104Body)tlv104._tlvBody)._sigSession;
                var sigCaptchaURL = ((T192Body)tlv192._tlvBody)._url;

                CallEvent<UserSigInfo>(new EventUpdateChallengeInfo
                {
                    Session = sigSession
                });

                PostEvent<ToUser>(new EventCaptchaCtl
                {
                    SliderUrl = sigCaptchaURL,
                    Type = EventCaptchaCtl.CtlType.Slider
                });
            }
            return false;
        }

        private bool Handle_VerifySmsCaptcha(Core core, OicqRequest request)
        {
            Console.WriteLine("Do sms verification.");

            var tlvs = request.oicqRequestBody.TakeAllBytes(out var _);
            var unpacker = new TlvUnpacker(tlvs, true);

            if (unpacker.Count == 8 || unpacker.Count == 9)
            {
                Tlv tlv104 = unpacker.TryGetTlv(0x104);
                Tlv tlv174 = unpacker.TryGetTlv(0x174);
                Tlv tlv204 = unpacker.TryGetTlv(0x204);
                Tlv tlv178 = unpacker.TryGetTlv(0x178);
                Tlv tlv179 = unpacker.TryGetTlv(0x179);
                Tlv tlv17d = unpacker.TryGetTlv(0x17d);
                Tlv tlv402 = unpacker.TryGetTlv(0x402);
                Tlv tlv403 = unpacker.TryGetTlv(0x403);
                Tlv tlv17e = unpacker.TryGetTlv(0x17e);

                if (tlv104 != null && tlv174 != null
                    && tlv204 != null && tlv178 != null
                    && tlv17d != null && tlv402 != null
                    && tlv403 != null && tlv17e != null)
                {
                    var sigSession = ((T104Body)tlv104._tlvBody)._sigSession;
                    var sigMessage = ((T17eBody)tlv17e._tlvBody)._message;
                    var smsPhone = ((T178Body)tlv178._tlvBody)._phone;
                    var smsCountryCode = ((T178Body)tlv178._tlvBody)._countryCode;
                    var smsToken = ((T174Body)tlv174._tlvBody)._smsToken;
                    Console.WriteLine($"[Hint] {sigMessage}");

                    CallEvent<UserSigInfo>(new EventUpdateChallengeInfo
                    {
                        Session = sigSession,
                        SmsPhone = smsPhone,
                        SmsToken = smsToken
                    });

                    PostEvent<ToUser>(new EventCaptchaCtl
                    {
                        SmsPhoneNumber = smsPhone,
                        SmsPhoneCountryCode = smsCountryCode,
                        Type = EventCaptchaCtl.CtlType.Sms
                    });

                    return true;
                }
            }
            else if (unpacker.Count == 2)
            {
                Tlv tlv104 = unpacker.TryGetTlv(0x104);
                Tlv tlv17b = unpacker.TryGetTlv(0x17b);

                if (tlv104 != null && tlv17b != null)
                {
                    var sigSession = ((T104Body)tlv104._tlvBody)._sigSession;

                    core.SigInfo.WtLoginSession = sigSession;
                    core.PostUserEvent(EventType.WtLoginVerifySmsCaptcha, core.SigInfo.WtLoginSmsPhone);

                    return true;
                }
            }
            else
            {
                core.PostSystemEvent(EventType.LoginFailed);
                Console.WriteLine("[Error] Unknown data received.");
            }

            return false;
        }

        private bool Handle_VerifyImageCaptcha(Core core, OicqRequest request)
        {
            // <TODO> Image captcha
            return false;
        }

        private bool Handle_VerifyDeviceLock(Core core, OicqRequest request)
        {
            Console.WriteLine("Do DeviceLock verification.");
            core.PostSystemEvent(EventType.LoginFailed);
            return false;
        }

        private bool Handle_WtloginSuccess(Core core, OicqRequest request)
        {
            Console.WriteLine("Wtlogin success.");

            var tlvs = request.oicqRequestBody.TakeAllBytes(out var _);
            var unpacker = new TlvUnpacker(tlvs, true);

            if (unpacker.Count == 2)
            {
                Tlv tlv119 = unpacker.TryGetTlv(0x119);
                Tlv tlv161 = unpacker.TryGetTlv(0x161);

                if (tlv119 != null && tlv161 != null)
                {
                    var decrtpted = tlv119._tlvBody.TakeDecryptedBytes(out var _,
                        TeaCryptor.Instance, core.SigInfo.TgtgKey);

                    var tlv119Unpacker = new TlvUnpacker(decrtpted, true);

                    Tlv tlv16a = tlv119Unpacker.TryGetTlv(0x16a); // no pic sig
                    Tlv tlv106 = tlv119Unpacker.TryGetTlv(0x106);
                    Tlv tlv10c = tlv119Unpacker.TryGetTlv(0x10c); // gt key
                    Tlv tlv10a = tlv119Unpacker.TryGetTlv(0x10a); // tgt
                    Tlv tlv10d = tlv119Unpacker.TryGetTlv(0x10d); // tgt key
                    Tlv tlv114 = tlv119Unpacker.TryGetTlv(0x114); // st
                    Tlv tlv10e = tlv119Unpacker.TryGetTlv(0x10e); // st key
                    Tlv tlv103 = tlv119Unpacker.TryGetTlv(0x103); // stwx_web
                    Tlv tlv133 = tlv119Unpacker.TryGetTlv(0x133);
                    Tlv tlv134 = tlv119Unpacker.TryGetTlv(0x134); // ticket key
                    Tlv tlv528 = tlv119Unpacker.TryGetTlv(0x528);
                    Tlv tlv322 = tlv119Unpacker.TryGetTlv(0x322); // device token
                    Tlv tlv11d = tlv119Unpacker.TryGetTlv(0x11d); // st, st key
                    Tlv tlv11f = tlv119Unpacker.TryGetTlv(0x11f);
                    Tlv tlv138 = tlv119Unpacker.TryGetTlv(0x138);
                    Tlv tlv11a = tlv119Unpacker.TryGetTlv(0x11a); // age, sex, nickname
                    Tlv tlv522 = tlv119Unpacker.TryGetTlv(0x522);
                    Tlv tlv537 = tlv119Unpacker.TryGetTlv(0x537);
                    Tlv tlv550 = tlv119Unpacker.TryGetTlv(0x550);
                    Tlv tlv203 = tlv119Unpacker.TryGetTlv(0x203);
                    Tlv tlv120 = tlv119Unpacker.TryGetTlv(0x120); // skey
                    Tlv tlv16d = tlv119Unpacker.TryGetTlv(0x16d);
                    Tlv tlv512 = tlv119Unpacker.TryGetTlv(0x512); // Map<domain, p_skey>
                    Tlv tlv305 = tlv119Unpacker.TryGetTlv(0x305); // d2key
                    Tlv tlv143 = tlv119Unpacker.TryGetTlv(0x143); // d2
                    Tlv tlv118 = tlv119Unpacker.TryGetTlv(0x118);
                    Tlv tlv163 = tlv119Unpacker.TryGetTlv(0x163);
                    Tlv tlv130 = tlv119Unpacker.TryGetTlv(0x130);
                    Tlv tlv403 = tlv119Unpacker.TryGetTlv(0x403);

                    var noPicSig = ((T16aBody)tlv16a._tlvBody)._noPicSig;

                    var tgtKey = ((T10dBody)tlv10d._tlvBody)._tgtKey;
                    var tgtToken = ((T10aBody)tlv10a._tlvBody)._tgtToken;

                    var d2Key = ((T305Body)tlv305._tlvBody)._d2Key;
                    var d2Token = ((T143Body)tlv143._tlvBody)._d2Token;

                    var wtSessionTicketSig = ((T133Body)tlv133._tlvBody)._wtSessionTicketSig;
                    var wtSessionTicketKey = ((T134Body)tlv134._tlvBody)._wtSessionTicketKey;

                    var gtKey = ((T10cBody)tlv10c._tlvBody)._gtKey;
                    var stKey = ((T10eBody)tlv10e._tlvBody)._stKey;

                    var userAge = ((T11aBody)tlv11a._tlvBody)._age;
                    var userFace = ((T11aBody)tlv11a._tlvBody)._face;
                    var userNickname = ((T11aBody)tlv11a._tlvBody)._nickName;

                    core.SsoMan.DestroyServiceSequence(name);

                    BroadcastEvent(new UserSigInfo.EventUpdateSigInfo
                    {
                        TgtKey = tgtKey,
                        TgtToken = tgtToken,

                        D2Key = d2Key,
                        D2Token = d2Token,

                        GtKey = gtKey,
                        StKey = stKey,

                        WtSessionTicketSig = wtSessionTicketSig,
                        WtSessionTicketKey = wtSessionTicketKey,

                        UinInfo = new UserSigInfo.EventUpdateSigInfo.Info
                        {
                            Age = userAge,
                            Face = userFace,
                            Name = userNickname
                        }
                    });

                    Console.WriteLine($"gtKey => {Hex.Bytes2HexStr(gtKey)}");
                    Console.WriteLine($"stKey => {Hex.Bytes2HexStr(stKey)}");
                    Console.WriteLine($"tgtKey => {Hex.Bytes2HexStr(tgtKey)}");
                    Console.WriteLine($"tgtToken => {Hex.Bytes2HexStr(tgtToken)}");
                    Console.WriteLine($"d2Key => {Hex.Bytes2HexStr(d2Key)}");
                    Console.WriteLine($"d2Token => {Hex.Bytes2HexStr(d2Token)}");

                    return true;
                }
            }

            return false;
        }

        private bool Handle_InvalidUserOrPassword(Core core, OicqRequest request)
        {
            BroadcastEvent(new EventOffline
            {
                Reason = "Incorrect account or password."
            });

            return false;
        }

        private bool Handle_InvalidSmsCode(Core core, OicqRequest request)
        {
            BroadcastEvent(new EventOffline
            {
                Reason = "Incorrect sms code."
            });

            return false;
        }

        private bool Handle_InvalidEnvironment(Core core, OicqRequest request)
        {
            BroadcastEvent(new EventOffline
            {
                Reason = "Invalid login environment."
            });

            var tlvs = request.oicqRequestBody.TakeAllBytes(out var _);
            var unpacker = new TlvUnpacker(tlvs, true);

            Tlv tlv146 = unpacker.TryGetTlv(0x146);
            if (tlv146 != null)
            {
                var errorTitle = ((T146Body)tlv146._tlvBody)._title;
                var errorMessage = ((T146Body)tlv146._tlvBody)._message;

                Console.WriteLine($"[Error] {errorTitle} {errorMessage}");
            }

            core.PostSystemEvent(EventType.LoginFailed);
            return false;
        }

        private bool Handle_LoginDenied(Core core, OicqRequest request)
        {
            BroadcastEvent(new EventOffline
            {
                Reason = "Login denied."
            });

            var tlvs = request.oicqRequestBody.TakeAllBytes(out var _);
            var unpacker = new TlvUnpacker(tlvs, true);

            Tlv tlv146 = unpacker.TryGetTlv(0x146);
            if (tlv146 != null)
            {
                var errorTitle = ((T146Body)tlv146._tlvBody)._title;
                var errorMessage = ((T146Body)tlv146._tlvBody)._message;

                Console.WriteLine($"[Error] {errorTitle} {errorMessage}");
            }

            core.PostSystemEvent(EventType.LoginFailed);
            return false;
        }

        private bool Handle_UnknownOicqRequest(Core core, OicqRequest request)
        {
            BroadcastEvent(new EventOffline
            {
                Reason = "Unknown OicqRequest received."
            });

            return false;
        }

        #endregion
    }
}
