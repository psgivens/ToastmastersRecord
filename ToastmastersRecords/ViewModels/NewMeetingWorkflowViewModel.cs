using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecords.Data;
using ToastmastersRecords.Infrastructure;
using ToastmastersRecords.ViewModels.History;
using ToastmastersRecords.ViewModels.Scheduler;
using ToastmastersRecords.ViewModels.SchedulingSteps;

namespace ToastmastersRecords.ViewModels {
    public class NewMeetingWorkflowViewModel : DataViewModelBase {
        private IList<MeetingRoleType> _roleTypes;
        private IList<MemberToSchedule> _members;

        public NewMeetingWorkflowViewModel(Data.TIDbContext context) : base(context) {
            _roleTypes = context.MeetingRoleTypes.ToList();
            var lowerDate = DateTime.Now - TimeSpan.FromDays(28);
            _members = (from m in context.Members
                        select new MemberToSchedule {
                            Member = m,
                            History = (from ra in context.RoleAssignments
                                       where ra.Member == m && ra.Date > lowerDate
                                       select ra).ToList()
                        }).ToList();
            Meeting = new MeetingViewModel();
            SchedulingStep = new ScheduleSpeakersViewModel(this);
            Members = new ObservableCollection<MemberToSchedule>();
            foreach(var member in _members) {
                Samples.Add(new MemberHistory(member.Member, member.History.ToDictionary(i => i.Date)));
            }
            
            FilterMembers();
        }

        public ObservableCollection<MemberHistory> Samples { get; private set; } = new ObservableCollection<MemberHistory>();

        public MeetingViewModel Meeting { get; private set; }

        public ViewModelBase SchedulingStep { get; private set; }

        public ObservableCollection<MemberToSchedule> Members { get; private set; }
        
        private void ScheduleMember(ClubMember member, string speakerRole) {
            var role = _roleTypes.First(r => r.Role == speakerRole);
            var sMember = _members.First(i => i.Member == member);
            sMember.SelectedRole = role;
            Members.Remove(sMember);
        }
        private void FilterMembers(int qualifyQty = 0) {
            Members.Clear();
            foreach (var memberMeta in _members) {
                var m = memberMeta.Member;
                if (memberMeta.SelectedRole != null) continue;
                if (!m.HasCompetentCommunicator && m.SpeechesSinceAward < qualifyQty) continue;
                Members.Add(memberMeta);
            }
        }
        public void SelectSpeakers(ClubMember speaker1, ClubMember speaker2, ClubMember speaker3) {
            Meeting.Speaker1 = speaker1;
            Meeting.Speaker2 = speaker2;
            Meeting.Speaker3 = speaker3;

            var speaker = "Speaker";
            ScheduleMember(speaker1, speaker);
            ScheduleMember(speaker2, speaker);
            ScheduleMember(speaker3, speaker);
        }
        public void SelectToastmaster(ClubMember member) {
            Meeting.Toastmaster = member;
            ScheduleMember(member, "Toastmaster");
        }
        public void SelectGeneralEvaluator(ClubMember member) {
            Meeting.GeneralEvaluator = member;
            ScheduleMember(member, "General Evaluator");
        }
        public void SelectTableTopicsMaster(ClubMember member) {
            Meeting.TableTopicsMaster = member;
            ScheduleMember(member, "Table Topics Master");
        }
        public void SelectEvaluators(ClubMember evaluator1, ClubMember evaluator2, ClubMember evaluator3) {
            Meeting.Evaluator1 = evaluator1;
            ScheduleMember(evaluator1, "Evaluator");
            Meeting.Evaluator2 = evaluator2;
            ScheduleMember(evaluator2, "Evaluator");
            Meeting.Evaluator3 = evaluator3;
            ScheduleMember(evaluator3, "Evaluator");
        }
        public void SelectMinorRoles(ClubMember openingThought, ClubMember jokeMaster, ClubMember closingThought) {
            Meeting.OpeningThought = openingThought;
            ScheduleMember(openingThought, "Opening Thought and Ballot Counter");
            Meeting.JokeMaster = jokeMaster; ;
            ScheduleMember(jokeMaster, "Joke Master");
            Meeting.ClosingThought = closingThought;
            ScheduleMember(closingThought, "Closing Thought and Greeter");
        }
    }
}
