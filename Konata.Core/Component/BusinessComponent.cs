using System;
using System.Text;
using System.Threading.Tasks;
using Konata.Core.Entity;
using Konata.Core.Event;

namespace Konata.Core.Manager
{
    [Component("BusinessComponent", "Konata Business Component")]
    public class BusinessComponent : BaseComponent
    {
        public string TAG = "BusinessComponent";

        public async Task<bool> Login()
            => await LoginLogic((WtLoginEvent)await PostEvent<PacketComponent>
                (new WtLoginEvent { EventType = WtLoginEvent.Type.Tgtgt }));

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

        public override void EventHandler(KonataTask task)
        {
            if (task.EventPayload is WtLoginEvent wtloginEvent)
            {
                LoginLogic(wtloginEvent).Wait();
            }

            //else if ()
            //{

            //}

            else LogW(TAG, "Unsupported event received.");

        }
    }
}
