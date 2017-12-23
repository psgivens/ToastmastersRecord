using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data
{
    public abstract class EnvelopeEntityBase {
        [Column(Order = 0), Key]
        public virtual Guid StreamId { get; set; }
        [Column(Order = 1), Key]
        public virtual Guid UserId { get; set; }
        [Column(Order = 2), Key]
        public virtual Guid Id { get; set; }

        public virtual Guid TransactionId { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual short Version { get; set; }        
        public virtual DateTimeOffset TimeStamp { get; set; }
        public virtual string Event { get; set; }
    }
    public class RoleRequestEnvelopeEntity : EnvelopeEntityBase { }
    public class RolePlacementEnvelopeEntity : EnvelopeEntityBase { }
    public class MemberEnvelopeEntity : EnvelopeEntityBase { }
    public class ClubMeetingEnvelopeEntity : EnvelopeEntityBase { }
}
