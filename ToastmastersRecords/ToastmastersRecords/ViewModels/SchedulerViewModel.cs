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
    public class SchedulerViewModel : DataViewModelBase {
        private readonly TIDbContext context;
        private MeetingViewModel _selectedSchedule;
        private readonly ObservableCollection<ClubMeeting> _clubMeetings;
        public ObservableCollection<ClubMeeting> ClubMeetings { get { return _clubMeetings; } }
        public MeetingViewModel SelectedSchedule {
            get { return _selectedSchedule; }
            set {
                _selectedSchedule = value;
                Notify("SelectedSchedule");
            }
        }

        public SchedulerViewModel(TIDbContext context) : base(context) {
            this.context = context;
            _clubMeetings = new ObservableCollection<ClubMeeting>(context.ClubMeetings);
        }

    }
}
