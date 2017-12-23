using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;
using ToastmastersRecords.Infrastructure;

namespace ToastmastersRecords.ViewModels {
    public class RoleRequestsViewModel : DataViewModelBase {
        private readonly ClubMember _member;
        private readonly MemberMessage _message;
        public RoleRequestsViewModel(TIDbContext context, ClubMember member) : base(context) {
            _member = member;

            RoleTypes = new ObservableCollection<MeetingRoleType>(context.MeetingRoleTypes);
            RoleRequests = (
                from req in Context.RoleRequests
                where req.Message.MemberId == _member.Id
                select req).ToList();
        }
        public RoleRequestsViewModel(TIDbContext context, MemberMessage message) : base(context) {
            _message = message;

            RoleTypes = new ObservableCollection<MeetingRoleType>(context.MeetingRoleTypes);
            RoleRequests = (
                from req in Context.RoleRequests
                where req.MessageId == message.Id
                select req).ToList();
        }

        public ObservableCollection<MeetingRoleType> RoleTypes { get; set; }
        private IList<RoleRequest> _roleRequests;
        public IList<RoleRequest> RoleRequests {
            get { return _roleRequests; }
            set {
                _roleRequests = value;
                Notify("RoleRequests");
            }
        }
        public RoleRequest CreateRoleRequest() {
            MemberMessage message;
            if (_message == null) {
                message = Context.MemberMessages.Create();
                message.Member = _member;
                message.DateEntered = DateTime.Now;
            } else {
                message = _message;
            }
            var req = Context.RoleRequests.Create();
            req.Message = message;
            req.DateRequested = DateTime.Now;

            return req;
        }

    }
}
