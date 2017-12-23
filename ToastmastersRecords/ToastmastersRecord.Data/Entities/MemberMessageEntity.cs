using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class MemberMessageEntity {
        [Key]
        public virtual Guid Id { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime MessageDate { get; set; }
        public virtual string Message { get; set; }
        public virtual Guid MemberId { get; set; }
        public virtual MemberEntity Member { get; set; }
    }
}
