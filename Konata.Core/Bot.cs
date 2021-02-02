using System;
using System.Threading.Tasks;

using Konata.Core.Event.EventModel;
using Konata.Core.Entity;
using Konata.Core.Component;

namespace Konata.Core
{
    public class Bot : BaseEntity
    {
        /// <summary>
        /// Login
        /// </summary>
        public Task<bool> Login()
            => GetComponent<BusinessComponent>().Login();

        /// <summary>
        /// Submit Slider ticket
        /// </summary>
        /// <param name="ticket"><b>[In]</b> Slider ticket</param>
        public Task<bool> SubmitSliderTicket(string ticket)
            => GetComponent<BusinessComponent>().SubmitSliderTicket(ticket);

        /// <summary>
        /// Submit SMS code.
        /// </summary>
        /// <param name="code"><b>[In]</b> SMS code</param>
        public Task<bool> SubmitSMSCode(string code)
            => GetComponent<BusinessComponent>().SubmitSMSCode(code);

        /// <summary>
        /// Kick a member in the specific group.
        /// </summary>
        /// <param name="groupUin"><b>[In]</b> Group uin being operated. </param>
        /// <param name="memberUin"><b>[In]</b> Member uin being operated. </param>
        /// <param name="preventRequest"><b>[In]</b> Flag to prevent member request or no. </param>
        public Task<GroupKickMemberEvent> GroupKickMember(uint groupUin, uint memberUin, bool preventRequest)
            => GetComponent<BusinessComponent>().GroupKickMember(groupUin, memberUin, preventRequest);

        /// <summary>
        /// Promote a member to admin in the specific group.
        /// </summary>
        /// <param name="groupUin"><b>[In]</b> Group uin being operated. </param>
        /// <param name="memberUin"><b>[In]</b> Member uin being operated. </param>
        /// <param name="toggleAdmin"><b>[In]</b> Flag to toggle set or unset. </param>
        public Task<GroupPromoteAdminEvent> GroupPromoteAdmin(uint groupUin, uint memberUin, bool toggleAdmin)
            => GetComponent<BusinessComponent>().GroupPromoteAdmin(groupUin, memberUin, toggleAdmin);
    }
}
