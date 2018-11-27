using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;
using ToastmastersRecords.Infrastructure;

namespace ToastmastersRecords.ViewModels {
    public class MembersViewModel : DataViewModelBase {
        public MembersViewModel(TIDbContext context) : base(context) {            
            Members = context.Members.ToList();
            Member = Members.First();
        }

        private DayOffRequestsViewModel _dayOffRequestsViewModel;
        public DayOffRequestsViewModel DayOffRequestsVM {
            get { return _dayOffRequestsViewModel; }
            set { _dayOffRequestsViewModel = value; Notify("DayOffRequestsVM"); }
        }
        
        private RoleRequestsViewModel _roleRequestsViewModel;
        public RoleRequestsViewModel RoleRequestsVM {
            get { return _roleRequestsViewModel; }
            set { _roleRequestsViewModel = value; Notify("RoleRequestsVM"); }
        }

        private ClubMember _member;
        public ClubMember Member {
            get {
                return _member;
            }
            set {
                _member = value;
                ReloadUserInfo();
            }
        }
        public IList<ClubMember> Members { get; private set; }
                
        private IList<RoleAssignment> _roleAssignments;
        public IList<RoleAssignment> RoleAssignments {
            get { return _roleAssignments; }
            set {
                _roleAssignments = value;
                Notify("RoleAssignment");
            }
        }

        private MemberMessageViewModel _memberMessageViewModel;
        public MemberMessageViewModel MemberMessageVM {
            get { return _memberMessageViewModel; }
            set { _memberMessageViewModel = value; Notify("MemberMessageVM"); }
        }

        public void ReloadUserInfo() {
            if (_member == null) {
                ClearUserInfo();
            }
            else {
                LoadUserInfo();
            }
        }        
        private void LoadUserInfo() {
            DayOffRequestsVM = new DayOffRequestsViewModel(Context, _member);
            RoleRequestsVM = new RoleRequestsViewModel(Context, _member);
            RoleAssignments = (
                from assign in Context.RoleAssignments
                where assign.MemberId == _member.Id
                select assign).ToList();
        }

        private void ClearUserInfo() {
            DayOffRequestsVM.DayOffRequests = null;
            RoleRequestsVM.RoleRequests = null;
            RoleAssignments = null;
        }
        
        public RoleAssignment CreateRoleAssignment() {
            var assignment = Context.RoleAssignments.Create();
            assignment.Member = _member;
            assignment.Date = DateTime.Now;

            return assignment;
        }

    }
}

