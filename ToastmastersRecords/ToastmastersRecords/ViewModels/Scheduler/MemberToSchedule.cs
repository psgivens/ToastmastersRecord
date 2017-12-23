using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;

namespace ToastmastersRecords.ViewModels.Scheduler {
    public class MemberToSchedule {
        
        public ClubMember Member { get; set; }
        public IEnumerable<RoleAssignment> History { get; set; }
        public IList<RoleRequest> Requests { get; set; }
        public MeetingRoleType SelectedRole { get; set; }
    }
}
