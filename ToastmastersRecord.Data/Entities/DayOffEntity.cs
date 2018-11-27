using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class DayOffEntity {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual Guid MessageId { get; set; }        
        public virtual Guid MemberId { get; set; }
        public virtual Guid MeetingId { get; set; }
    }
}
