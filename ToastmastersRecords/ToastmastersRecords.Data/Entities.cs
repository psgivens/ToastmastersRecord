using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.Data {

    public class ClubMember {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int SpeechesSinceAward { get; set; }
        public virtual bool HasCompetentCommunicator { get; set; }
        public virtual string Email { get; set; }
        public virtual string HomePhone { get; set; }
        public virtual string MobilePhone { get; set; }
        public virtual string OtherPhone { get; set; }
        [Column(TypeName = "Date")]
        public virtual DateTime MemberSince { get; set; }
        [Column(TypeName = "Date")]
        public virtual DateTime JoinedClub { get; set; }

    }
    public class BaseEntity {
        [Key]
        public virtual int Id { get; set; }
    }
    public class MemberSpeech : BaseEntity {
        public virtual int MemberId { get; set; }
        public virtual ClubMember Member { get; set; }
        [Column(TypeName = "Date")]
        public virtual DateTime DateDelivered { get; set; }
        public virtual string Title { get; set; }
        public virtual string Manual { get; set; }
        public virtual string Assignment { get; set; }
        public virtual int AssignmentNumber { get; set; }
    }
    public class MemberTitle : BaseEntity {
        public virtual int MemberId { get; set; }
        public virtual ClubMember Member { get; set; }
        public virtual string Title { get; set; }
    }
    public class ClubMeeting : BaseEntity {
        [Column(TypeName = "Date")]
        public virtual DateTime MeetingDate { get; set; }
        public virtual string Theme { get; set; }
    }
    public class MeetingRoleType {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        public virtual string Role { get; set; }
        public virtual int Miniumum { get; set; }
    }
    public class MemberMessage : BaseEntity {
        public virtual int MemberId { get; set; }
        public virtual ClubMember Member { get; set; }
        [Column(TypeName = "Date")]
        public virtual DateTime DateEntered { get; set; }
        public virtual string Text { get; set; }
    }
    public class DayOffRequest : BaseEntity {
        [Column(TypeName = "Date")]
        public virtual DateTime? DateRequested { get; set; }
        public virtual int MessageId { get; set; }
        public virtual MemberMessage Message { get; set; }
    }
    public class RoleRequest : DayOffRequest {
        public virtual int RoleTypeId { get; set; }
        public virtual MeetingRoleType RoleType { get; set; }
        public virtual RoleAssignment Assigned { get; set; }
    }
    public class RoleAssignment : BaseEntity {
        public virtual int MemberId { get; set; }
        public virtual ClubMember Member { get; set; }
        [Column(TypeName = "Date")]
        public virtual DateTime Date { get; set; }
        public virtual int RoleTypeId { get; set; }
        public virtual MeetingRoleType RoleType { get; set; }
    }
}
