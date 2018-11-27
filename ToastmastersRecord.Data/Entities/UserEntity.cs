using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class UserEntity {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
    }
}
