using System;
using System.Text;
using System.Threading.Tasks;

using Konata.Core.Event;
using Konata.Core.Event.EventModel;
using Konata.Core.Entity;

namespace Konata.Core.Component
{
    [Component("BusinessComponent", "Konata Business Component")]
    public class BusinessComponent : BaseComponent
    {
        public string TAG = "BusinessComponent";

        private OnlineStatusEvent.Type _onlineType;
        private TaskCompletionSource<WtLoginEvent> _pendingUserOperation;

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

                var wtStatus = await WtLogin();
                {
                    while (wtStatus == null || wtStatus.EventType != WtLoginEvent.Type.OK)
                    {
                        switch (wtStatus.EventType)
                        {
                            case WtLoginEvent.Type.OK:
                                return true;

                            case WtLoginEvent.Type.CheckSMS:
                            case WtLoginEvent.Type.CheckSlider:
                                PostEventToEntity(wtStatus);
                                wtStatus = await WtCheckUserOperation();
                                break;

                            case WtLoginEvent.Type.RefreshSMS:
                                wtStatus = await WtRefreshSMSCode();
                                break;

                            case WtLoginEvent.Type.CheckDevLock:
                            //wtStatus = await WtValidateDeviceLock();
                            //break;

                            case WtLoginEvent.Type.LoginDenied:
                            case WtLoginEvent.Type.InvalidSmsCode:
                            case WtLoginEvent.Type.InvalidLoginEnvironment:
                            case WtLoginEvent.Type.InvalidUinOrPassword:
                                PostEventToEntity(wtStatus);
                                return false;

                            default:
                            case WtLoginEvent.Type.NotImplemented:
                                LogW(TAG, "Login fail. Unsupported wtlogin event type received.");
                                return false;
                        }
                    }
                }

                LogW(TAG, "You goes here? What the happend?");
                return false;
            }

            LogW(TAG, "Calling Login method again while online.");
            return false;
        }

        public async void SubmitSMSCode(string code)
            => _pendingUserOperation.SetResult(new WtLoginEvent
            { EventType = WtLoginEvent.Type.CheckSMS, CaptchaResult = code });

        public async void SubmitSliderTicket(string ticket)
            => _pendingUserOperation.SetResult(new WtLoginEvent
            { EventType = WtLoginEvent.Type.CheckSlider, CaptchaResult = ticket });

        internal async Task<WtLoginEvent> WtLogin()
            => (WtLoginEvent)await PostEvent<PacketComponent>
            (new WtLoginEvent { EventType = WtLoginEvent.Type.Tgtgt });

        internal async Task<WtLoginEvent> WtRefreshSMSCode()
            => (WtLoginEvent)await PostEvent<PacketComponent>
            (new WtLoginEvent { EventType = WtLoginEvent.Type.RefreshSMS });

        internal async Task<WtLoginEvent> WtValidateDeviceLock()
            => (WtLoginEvent)await PostEvent<PacketComponent>
            (new WtLoginEvent { EventType = WtLoginEvent.Type.CheckDevLock });

        internal async Task<WtLoginEvent> WtCheckUserOperation()
            => (WtLoginEvent)await PostEvent<PacketComponent>
            (await WaitForUserOperation());

        private async Task<WtLoginEvent> WaitForUserOperation()
        {
            // _pendingUserOperation?.SetCanceled();
            _pendingUserOperation = new TaskCompletionSource<WtLoginEvent>();
            return await _pendingUserOperation.Task;
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
            if (task.EventPayload is OnlineStatusEvent onlineStatusEvent)
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
