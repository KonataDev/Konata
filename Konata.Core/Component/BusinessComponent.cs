using System;
using System.Text;
using System.Threading.Tasks;

using Konata.Core.Entity;
using Konata.Core.Event.EventModel;

namespace Konata.Core.Component
{
    [Component("BusinessComponent", "Konata Business Component")]
    public class BusinessComponent : BaseComponent
    {
        public string TAG = "BusinessComponent";

        private OnlineStatusEvent.Type _onlineType;

        public BusinessComponent()
        {
            _onlineType = OnlineStatusEvent.Type.Offline;
        }

        public async Task<bool> Login()
        {
            if (_onlineType == OnlineStatusEvent.Type.Offline)
            {
                if (!await GetComponent<SocketComponent>().Connect(true))
                {
                    return false;
                }

                var result = await PostEvent<PacketComponent>
                     (new WtLoginEvent { EventType = WtLoginEvent.Type.Tgtgt });

                return await LoginLogic((WtLoginEvent)result);
            }

            LogW(TAG, "Calling Login method again while online.");
            return false;
        }

        public async Task<bool> RefreshSMSCode()
        => await LoginLogic((WtLoginEvent)await PostEvent<PacketComponent>
                (new WtLoginEvent { EventType = WtLoginEvent.Type.RefreshSMS }));

        public async Task<bool> SubmitSMSCode(string code)
            => await LoginLogic((WtLoginEvent)await PostEvent<PacketComponent>
                (new WtLoginEvent { EventType = WtLoginEvent.Type.CheckSMS, CaptchaResult = code }));

        public async Task<bool> SubmitSliderTicket(string ticket)
            => await LoginLogic((WtLoginEvent)await PostEvent<PacketComponent>
                (new WtLoginEvent { EventType = WtLoginEvent.Type.CheckSlider, CaptchaResult = ticket }));

        public async Task<bool> ValidateDeviceLock()
            => await LoginLogic((WtLoginEvent)await PostEvent<PacketComponent>
                (new WtLoginEvent { EventType = WtLoginEvent.Type.CheckDevLock }));

        private async Task<bool> LoginLogic(WtLoginEvent wtEvent)
        {
            switch (wtEvent.EventType)
            {
                case WtLoginEvent.Type.OK:
                    return true;
                case WtLoginEvent.Type.CheckSMS:
                    return await RefreshSMSCode();
                case WtLoginEvent.Type.CheckSlider:
                    PostEventToEntity(wtEvent);
                    return false;
                case WtLoginEvent.Type.CheckDevLock:
                    return await ValidateDeviceLock();
                default: return false;
            }
        }

        public async Task<GroupKickMemberEvent> GroupKickMember(uint groupUin, uint memberUin, bool preventRequest)
            => (GroupKickMemberEvent)await PostEvent<PacketComponent>
                (new GroupKickMemberEvent
                {
                    GroupUin = groupUin,
                    MemberUin = memberUin,
                    ToggleType = preventRequest
                });

        public async Task<GroupPromoteAdminEvent> GroupPromoteAdmin(uint groupUin, uint memberUin, bool toggleAdmin)
            => (GroupPromoteAdminEvent)await PostEvent<PacketComponent>
                (new GroupPromoteAdminEvent
                {
                    GroupUin = groupUin,
                    MemberUin = memberUin,
                    ToggleType = toggleAdmin
                });

        internal override void EventHandler(KonataTask task)
        {
            if (task.EventPayload is WtLoginEvent wtloginEvent)
            {
                LoginLogic(wtloginEvent).Wait();
            }

            else if (task.EventPayload is OnlineStatusEvent onlineStatusEvent)
            {
                _onlineType = onlineStatusEvent.EventType;
            }

            //else if ()
            //{

            //}

            else LogW(TAG, "Unsupported event received.");
        }
    }
}
