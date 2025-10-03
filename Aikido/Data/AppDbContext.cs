using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Основные сущности
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
        public DbSet<StatementEntity> Statements { get; set; }

        // Промежуточные таблицы для many-to-many связей

        public DbSet<UserMembershipEntity> UserMemberships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация UserEntity
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Исключаем вычисляемое свойство из базы
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.SecondName).HasMaxLength(100);
                entity.Property(e => e.Login).HasMaxLength(50);
                entity.Property(e => e.Password).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.ParentFullName).HasMaxLength(200);
                entity.Property(e => e.ParentPhoneNumber).HasMaxLength(20);

                entity.Property(e => e.CertificationDates)
                    .HasConversion(
                        v => string.Join(';', v.Select(d => d.ToString("O"))),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                              .Select(DateTime.Parse).ToList());

                entity.Property(e => e.PaymentDates)
                    .HasConversion(
                        v => string.Join(';', v.Select(d => d.ToString("O"))),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                              .Select(DateTime.Parse).ToList());

                // Индексы
                entity.HasIndex(e => e.Login).IsUnique();
                entity.HasIndex(e => e.PhoneNumber);
            });

            modelBuilder.Entity<UserMembershipEntity>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.User)
                    .WithMany(u => u.UserMemberships)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Club)
                    .WithMany(c => c.UserMemberships)
                    .HasForeignKey(m => m.ClubId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Group)
                    .WithMany(g => g.UserMemberships)
                    .HasForeignKey(m => m.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // Конфигурация ClubEntity
            modelBuilder.Entity<ClubEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(200);

                // Связь с главным тренером
                entity.HasOne(c => c.Manager)
                    .WithMany()
                    .HasForeignKey(c => c.ManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.City);
            });

            // Конфигурация GroupEntity
            modelBuilder.Entity<GroupEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);

                // Связь с тренером
                entity.HasOne(g => g.Coach)
                    .WithMany()
                    .HasForeignKey(g => g.CoachId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с клубом
                entity.HasOne(g => g.Club)
                    .WithMany(c => c.Groups)
                    .HasForeignKey(g => g.ClubId)
                    .OnDelete(DeleteBehavior.SetNull);


                // Индексы
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.ClubId);
                entity.HasIndex(e => e.CoachId);
            });

            // Конфигурация EventEntity
            modelBuilder.Entity<EventEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(300);

                // Связь с группой
                entity.HasOne(e => e.Group)
                    .WithMany()
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с клубом
                entity.HasOne(e => e.Club)
                    .WithMany()
                    .HasForeignKey(e => e.ClubId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.GroupId);
                entity.HasIndex(e => e.ClubId);
            });

            // Остальные конфигурации...
            ConfigureSeminarEntity(modelBuilder);
            ConfigureSeminarMemberEntity(modelBuilder);
            ConfigureAttendanceEntity(modelBuilder);
            ConfigurePaymentEntity(modelBuilder);
            ConfigureScheduleEntity(modelBuilder);
            ConfigureExclusionDateEntity(modelBuilder);
            ConfigureStatementEntity(modelBuilder);
        }

        private void ConfigureSeminarEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SeminarEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(300);
                entity.Property(e => e.Address).HasMaxLength(400);
                entity.Property(e => e.InstructorName).HasMaxLength(200);
                entity.Property(e => e.ContactInfo).HasMaxLength(500);
                entity.Property(e => e.Cost).HasPrecision(18, 2);
                entity.Property(e => e.Materials)
                    .HasConversion(
                        v => string.Join(';', v),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList());

                // Связь с инструктором
                entity.HasOne(s => s.Instructor)
                    .WithMany()
                    .HasForeignKey(s => s.InstructorId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.InstructorId);
            });
        }

        private void ConfigureSeminarMemberEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SeminarMemberEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
                entity.Property(e => e.EmergencyContact).HasMaxLength(200);

                // Связи
                entity.HasOne(sm => sm.Seminar)
                    .WithMany(s => s.SeminarMembers)
                    .HasForeignKey(sm => sm.SeminarId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sm => sm.User)
                    .WithMany()
                    .HasForeignKey(sm => sm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Уникальный индекс - пользователь может быть участником семинара только один раз
                entity.HasIndex(e => new { e.SeminarId, e.UserId }).IsUnique();
            });
        }

        private void ConfigureAttendanceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttendanceEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Связи
                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Event)
                    .WithMany(e => e.Attendances)
                    .HasForeignKey(a => a.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Индексы
                entity.HasIndex(e => new { e.UserId, e.EventId, e.Date });
                entity.HasIndex(e => e.Date);
            });
        }

        private void ConfigurePaymentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                // Связи
                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ProcessedByUser)
                    .WithMany()
                    .HasForeignKey(p => p.ProcessedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Club)
                    .WithMany()
                    .HasForeignKey(p => p.ClubId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Group)
                    .WithMany()
                    .HasForeignKey(p => p.GroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы
                entity.HasIndex(e => e.PaymentDate);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TransactionId);
            });
        }

        private void ConfigureScheduleEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScheduleEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Связь с группой
                entity.HasOne(s => s.Group)
                    .WithMany(g => g.Schedule)
                    .HasForeignKey(s => s.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Индексы
                entity.HasIndex(e => new { e.GroupId, e.DayOfWeek, e.StartTime });
            });
        }

        private void ConfigureExclusionDateEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExclusionDateEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).HasMaxLength(500);

                // Связи
                entity.HasOne(ed => ed.Group)
                    .WithMany()
                    .HasForeignKey(ed => ed.GroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.GroupId);
            });
        }

        private void ConfigureStatementEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StatementEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(300);
                entity.Property(e => e.FilePath).HasMaxLength(500);

                // Связи
                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(s => s.Club)
                    .WithMany()
                    .HasForeignKey(s => s.ClubId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(s => s.Group)
                    .WithMany()
                    .HasForeignKey(s => s.GroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы
                entity.HasIndex(e => e.CreatedDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
            });
        }
    }
}