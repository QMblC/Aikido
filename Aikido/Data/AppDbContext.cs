using Aikido.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ClubEntity> Clubs { get; set; }
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<EventEntity> Events { get; set; }
        public DbSet<AttendanceEntity> Attendances { get; set; }
        public DbSet<ScheduleEntity> Schedule { get; set; }
        public DbSet<ExclusionDateEntity> ExclusionDates { get; set; }
        public DbSet<SeminarEntity> Seminars { get; set; }
        public DbSet<SeminarMemberEntity> SeminarMembers { get; set; }
        public DbSet<PaymentEntity> Payment { get; set; }
    }
}
