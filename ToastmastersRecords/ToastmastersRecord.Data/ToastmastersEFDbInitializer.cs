using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastmastersRecord.Data.Entities;

namespace ToastmastersRecord.Data {
    internal class ToastmastersEFDbInitializer : DropCreateDatabaseAlways<ToastmastersEFDbContext> { // DropCreateDatabaseIfModelChanges<ToastmastersEFDbContext> {
        protected override void Seed(ToastmastersEFDbContext context) {
            //var x = new RoleRequestEnvelopeEntity {
            //    StreamId = Guid.NewGuid(),
            //    TransactionId = Guid.NewGuid(),
            //    UserId = Guid.NewGuid().ToString(),
            //    DeviceId = Guid.NewGuid().ToString (),
            //    Version = 0,
            //    Id = Guid.NewGuid (),
            //    TimeStamp = DateTimeOffset.Now ,
            //    Event = ""
            //    };

            var nullMember = context.Members.Create();
            nullMember.Name = "No member selected";
            nullMember.PaidUntil = DateTime.Parse("1/1/1900");
            nullMember.ClubMemberSince = DateTime.Parse("1/1/1900");
            nullMember.OriginalJoinDate = DateTime.Parse("1/1/1900");
            context.Members.Add(nullMember);

            var nullMeeting = context.ClubMeetings.Create();
            nullMeeting.Theme = "Not a meeting";
            nullMeeting.Date = DateTime.Parse("1/1/1900");
            context.ClubMeetings.Add(nullMeeting);

            context.RoleTypes.AddRange(new RoleTypeEntity[] {
                new RoleTypeEntity { Id=1, Classification="Facilitator", MinimumSpeechCount=5, Title="Toastmaster" },
                new RoleTypeEntity { Id=2, Classification="Facilitator", MinimumSpeechCount=3, Title="Table Topics Master" },
                new RoleTypeEntity { Id=3, Classification="Facilitator", MinimumSpeechCount=4, Title="General Evaluator" },
                new RoleTypeEntity { Id=4, Classification="Prominant", MinimumSpeechCount=0, Title="Speaker" },
                new RoleTypeEntity { Id=5, Classification="Prominant", MinimumSpeechCount=1, Title="Evaluator" },
                new RoleTypeEntity { Id=6, Classification="Ancilliary", MinimumSpeechCount=0, Title="Joke Master" },
                new RoleTypeEntity { Id=7, Classification="Ancilliary", MinimumSpeechCount=0, Title="Opening Thought and Ballot Counter" },
                new RoleTypeEntity { Id=8, Classification="Ancilliary", MinimumSpeechCount=0, Title="Closing Thought and Greeter" },
                new RoleTypeEntity { Id=9, Classification="Functionary", MinimumSpeechCount=0, Title="Grammarian" },
                new RoleTypeEntity { Id=10, Classification="Functionary", MinimumSpeechCount=0, Title="Er, Ah Counter" },                
                new RoleTypeEntity { Id=11, Classification="Functionary", MinimumSpeechCount=0, Title="Timer" },
                new RoleTypeEntity { Id=12, Classification="Functionary", MinimumSpeechCount=0, Title="Videographer" }
                });
            
            context.Users.AddRange(new UserEntity[] {
                new UserEntity { Id=new Guid("{338D805A-E434-4ECA-BC11-022E9AEE820E}"), Name = "ToastmastersRecord.SampleApp.Initialize" },
                new UserEntity { Id=new Guid("{BFFF581A-A4C6-4816-88C9-DF8A7B00D90C}"), Name = "ToastmastersRecord.SampleApp.MessageProcessor" },
                new UserEntity { Id=new Guid("{2699A8E2-7787-4FFF-9FEF-348AE9B040A4}"), Name = "ToastmastersRecord.SampleApp.Schedule" },
                new UserEntity { Id=new Guid("{BE3B1456-4E94-45B5-A8E2-D12FC4C8573B}"), Name = "ToastmastersRecord.WPF" }
            });

            base.Seed(context);
        }
    }
}
