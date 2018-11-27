using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;
using ToastmastersRecords.Infrastructure;
using ToastmastersRecords.ViewModels.Scheduler;
using ToastmastersRecords.ViewModels.History;

namespace ToastmastersRecords {
    public class MeetingViewModel : ViewModelBase {
        
        
        private DateTime _selectedDate;
        public DateTime SelectedDate {
            get { return _selectedDate; }
            set {
                _selectedDate = value;
                Notify("SelectedDate");
            }
        }

        private IEnumerable<RoleAssignment> _assignments;
        public IEnumerable<RoleAssignment> Assignments {
            get { return _assignments; }
            set {
                _assignments = value;
                Notify("Assignments");
            }
        }

        private string _theme;
        public string Theme {
            get { return _theme; }
            set { _theme = value; Notify("Theme"); }
        }

        private ClubMember _toastmaster;
        public ClubMember Toastmaster {
            get { return _toastmaster; }
            set {
                _toastmaster = value;
                Notify("Toastmaster ");
            }
        }

        private ClubMember _tableTopicsMaster;
        public ClubMember TableTopicsMaster {
            get { return _tableTopicsMaster; }
            set {
                _tableTopicsMaster = value;
                Notify("TableTopicsMaster ");
            }
        }

        private ClubMember _generalEvaluator;
        public ClubMember GeneralEvaluator {
            get { return _generalEvaluator; }
            set {
                _generalEvaluator = value;
                Notify("GeneralEvaluator ");
            }
        }

        private ClubMember _jokeMaster;
        public ClubMember JokeMaster {
            get { return _jokeMaster; }
            set {
                _jokeMaster = value;
                Notify("JokeMaster ");
            }
        }

        private ClubMember _openingThought;
        public ClubMember OpeningThought {
            get { return _openingThought; }
            set {
                _openingThought = value;
                Notify("OpeningThought ");
            }
        }

        private ClubMember _closingThought;
        public ClubMember ClosingThought {
            get { return _closingThought; }
            set {
                _closingThought = value;
                Notify("ClosingThought ");
            }
        }

        private ClubMember _grammarian;
        public ClubMember Grammarian {
            get { return _grammarian; }
            set {
                _grammarian = value;
                Notify("Grammarian ");
            }
        }

        private ClubMember _fillerCounter;
        public ClubMember FillerCounter {
            get { return _fillerCounter; }
            set {
                _fillerCounter = value;
                Notify("FillerCounter ");
            }
        }

        private ClubMember _timer;
        public ClubMember Timer {
            get { return _timer; }
            set {
                _timer = value;
                Notify("Timer ");
            }
        }

        private ClubMember _videographer;
        public ClubMember Videographer {
            get { return _videographer; }
            set {
                _videographer = value;
                Notify("Videographer ");
            }
        }

        private ClubMember _evaluator1;
        public ClubMember Evaluator1 {
            get { return _evaluator1; }
            set {
                _evaluator1 = value;
                Notify("Evaluator1");
            }
        }

        private ClubMember _evaluator2;
        public ClubMember Evaluator2 {
            get { return _evaluator2; }
            set {
                _evaluator2 = value;
                Notify("Evaluator2");
            }
        }

        private ClubMember _evaluator3;
        public ClubMember Evaluator3 {
            get { return _evaluator3; }
            set {
                _evaluator3 = value;
                Notify("Evaluator3");
            }
        }

        private ClubMember _speaker1;
        public ClubMember Speaker1 {
            get { return _speaker1; }
            set {
                _speaker1 = value;
                Notify("Speaker1");
            }
        }

        private ClubMember _speaker2;
        public ClubMember Speaker2 {
            get { return _speaker2; }
            set {
                _speaker2 = value;
                Notify("Speaker2");
            }
        }

        private ClubMember _speaker3;
        public ClubMember Speaker3 {
            get { return _speaker3; }
            set {
                _speaker3 = value;
                Notify("Speaker3");
            }
        }
    }
}
