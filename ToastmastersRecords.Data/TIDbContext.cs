using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.Data {
    public class TIDbContext : DbContext{
        public virtual DbSet<ClubMember> Members { get; set; }
        public virtual DbSet<MeetingRoleType> MeetingRoleTypes { get; set; }
        public virtual DbSet<DayOffRequest> DayOffRequests { get; set; }
        public virtual DbSet<RoleRequest> RoleRequests { get; set; }
        public virtual DbSet<RoleAssignment> RoleAssignments { get; set; }
        public virtual DbSet<MemberMessage> MemberMessages { get; set; }
        public virtual DbSet<ClubMeeting> ClubMeetings { get; set; }
        public void Upsert(BaseEntity req) {
            Entry(req).State = req.Id == 0 ?
                               EntityState.Added :
                               EntityState.Modified;
        }
    }
}
