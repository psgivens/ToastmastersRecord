using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class MemberEntity {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual int ToastmasterId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Awards { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string Address5 { get; set; }
        public virtual string Country { get; set; }
        public virtual string Email { get; set; }
        public virtual string HomePhone { get; set; }
        public virtual string MobilePhone { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public virtual DateTime PaidUntil { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public virtual DateTime ClubMemberSince { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public virtual DateTime OriginalJoinDate { get; set; }
        public virtual string PaidStatus { get; set; }
        public virtual string CurrentPosition { get; set; }
        public virtual string FuturePosition { get; set; }
    }
}
