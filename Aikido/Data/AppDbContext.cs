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
    }
}
