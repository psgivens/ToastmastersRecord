using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Infrastructure;

namespace ToastmastersRecords.ViewModels.SchedulingSteps {
    public class ScheduleSpeakersViewModel : ScheduleRoleViewModelBase {
        public ScheduleSpeakersViewModel(NewMeetingWorkflowViewModel parent) : base(parent) { }
    }
}
