using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.Data {
    public class DatabaseInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<TIDbContext> {
        private readonly int IdField = 0;
        private readonly int NameField = 1;
        private readonly int EmailField = 9;
        private readonly int HomePhoneField = 11;
        private readonly int MobilePhoneField = 12;
        private readonly int AdditionalPhoneField = 13;
        private readonly int ClubSinceField = 15;
        private readonly int JoinDateField = 16;

        protected override void Seed(TIDbContext context) {
            foreach (var role in new[] {
                new MeetingRoleType { Id = RoleConstants.Toastmaster, Role="Toastmaster", Miniumum=5 },
                new MeetingRoleType { Id = RoleConstants.TableTopicsMaster, Role="Table Topics Master", Miniumum=3 },
                new MeetingRoleType { Id = RoleConstants.GeneralEvaluator, Role="General Evaluator", Miniumum=4 },
                new MeetingRoleType { Id = RoleConstants.Evaluator, Role="Evaluator", Miniumum=3 },
                new MeetingRoleType { Id = RoleConstants.OpeningThought, Role="Opening Thought and Ballot Counter"},
                new MeetingRoleType { Id = RoleConstants.JokeMaster, Role="Joke Master"},
                new MeetingRoleType { Id = RoleConstants.Grammarian, Role="Grammarian"},
                new MeetingRoleType { Id = RoleConstants.FillerCounter, Role="Er Ah Counter"},
                new MeetingRoleType { Id = RoleConstants.Timer, Role="Timer"},
                new MeetingRoleType { Id = RoleConstants.Speaker, Role="Speaker"},
                new MeetingRoleType { Id = RoleConstants.ClosingThought, Role="Closing Thought and Greeter"},
                new MeetingRoleType { Id = RoleConstants.Videographer, Role="Videographer"}
                }) {
                context.MeetingRoleTypes.Add(role);
            }
            using (var parser = new TextFieldParser(@"C:\temp\ClubRoster.csv")) {
                parser.Delimiters = new[] { "," };
                parser.ReadFields(); // skip 'sep=,'
                var fieldNames = parser.ReadFields();
                int i = 0;
                foreach (var name in fieldNames) {
                    Console.WriteLine("{0} {1}", i++, name);
                }
                while (true) {
                    var parts = parser.ReadFields();
                    if (parts == null)
                        break;

                    var who = parts[NameField].Split(',');
                    var member = new ClubMember {
                        Id = int.Parse(parts[IdField]),
                        Name = who[0],
                        HasCompetentCommunicator = who.Length > 1,
                        Email = parts[EmailField],
                        HomePhone = parts[HomePhoneField],
                        MobilePhone = parts[MobilePhoneField],
                        OtherPhone = parts[AdditionalPhoneField],
                        MemberSince = DateTime.Parse(parts[ClubSinceField]),
                        JoinedClub = DateTime.Parse(parts[JoinDateField])
                    };
                    context.Members.Add(member);
                    Console.WriteLine("{0} field(s)", parts.Length);
                }
            }

            context.SaveChanges();
        }
    }
}
