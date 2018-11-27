using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class RolePlacementEntity {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual Guid MemberId { get; set; }
//        public virtual MemberEntity Member { get; set; }
        public virtual Guid RoleRequestId { get; set; }
        public virtual int State { get; set; }
        public virtual int RoleTypeId { get; set; }        
        public virtual Guid MeetingId { get; set; }
//        public virtual ClubMeetingEntity Meeting { get; set; }
    }
}
