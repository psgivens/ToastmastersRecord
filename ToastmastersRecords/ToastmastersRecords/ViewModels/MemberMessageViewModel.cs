using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;

namespace ToastmastersRecords.ViewModels {
    public class MemberMessageViewModel : Infrastructure.DataViewModelBase {
        
        public MemberMessageViewModel(TIDbContext context, MemberMessage message) : base(context) {
            Load(message);
        }

        public MemberMessageViewModel(TIDbContext context, ClubMember member) : base(context) {
            var message = context.MemberMessages.Create();
            message.Member = member;
            message.DateEntered = DateTime.Now;
            Load(message);
        }

        private void Load(MemberMessage message) {
            Message = message;
            message.Text = "Sample text";
            DayOffRequestsVM = new DayOffRequestsViewModel(Context, message);
            RoleRequestsVM = new RoleRequestsViewModel(Context, message);
        }

        public MemberMessage Message { get; private set; }

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
    }
}
