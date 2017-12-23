using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;
using System.Reflection;

namespace ToastmastersRecords.ViewModels.History {
    public class MemberHistory  {
        private ClubMember _member;
        private IDictionary<DateTime, RoleAssignment> _roleAssignments;
        public MemberHistory(ClubMember member, IDictionary<DateTime, RoleAssignment> roleAssignments) {
            _member = member;
            Name = _member.Name;
            _roleAssignments = roleAssignments;
        }
        public string Name { get; private set; } 

        public string GetRole(DateTime date) {
            RoleAssignment assign;
            if (_roleAssignments.TryGetValue(date, out assign)) {
                return assign.RoleType.Role;
            }
            return "NA";
        }

    }
}
