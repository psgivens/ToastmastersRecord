using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Infrastructure;

namespace ToastmastersRecords.ViewModels.SchedulingSteps {
    public class ScheduleRoleViewModelBase : ViewModelBase {
        protected NewMeetingWorkflowViewModel Parent { get; private set; }
        public ScheduleRoleViewModelBase(NewMeetingWorkflowViewModel parent) {
            Parent = parent;
        }
    }
}
