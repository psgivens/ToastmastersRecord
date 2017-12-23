using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class RoleTypeEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Classification { get; set; }
        public virtual int MinimumSpeechCount { get; set; }
    }
}
