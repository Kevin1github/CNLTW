using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ConferenceDelegate> ConferenceDelegates { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Delegate1> Delegates { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionAttendance> SessionAttendances { get; set; } = null!;
        public DbSet<RestStop> RestStops { get; set; }
        public DbSet<RestStopBooking> RestStopBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Company).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.JobTitle).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.CCCD).IsRequired(false);
            });

            modelBuilder.Entity<Conference>()
                .Property(c => c.City)
                .HasColumnType("VARCHAR(255)");

            // Conference - Session relationship
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Conference)
                .WithMany(c => c.Sessions)
                .HasForeignKey(s => s.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Delegate - Registration relationship
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Delegate)
                .WithMany(d => d.Registrations)
                .HasForeignKey(r => r.DelegateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Conference - Registration relationship
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Conference)
                .WithMany(c => c.Registrations)
                .HasForeignKey(r => r.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Registration - SessionAttendance relationship
            modelBuilder.Entity<SessionAttendance>()
                .HasOne(sa => sa.Registration)
                .WithMany(r => r.SessionAttendances)
                .HasForeignKey(sa => sa.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Session - SessionAttendance relationship
            modelBuilder.Entity<SessionAttendance>()
                .HasOne(sa => sa.Session)
                .WithMany(s => s.Attendances)
                .HasForeignKey(sa => sa.SessionId)
                .OnDelete(DeleteBehavior.Restrict); // Use Restrict to avoid cascade delete conflicts

            modelBuilder.Entity<ConferenceDelegate>()
                .HasMany(d => d.Schedules)
                .WithOne(s => s.ConferenceDelegate)
                .HasForeignKey(s => s.ConferenceDelegateId);

            // Conference - RestStop relationship
            modelBuilder.Entity<RestStop>()
                .HasOne(r => r.Conference)
                .WithMany(c => c.RestStops)
                .HasForeignKey(r => r.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            // RestStop - RestStopBooking relationship
            modelBuilder.Entity<RestStopBooking>()
                .HasOne(b => b.RestStop)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RestStopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Registration - RestStopBooking relationship
            modelBuilder.Entity<RestStopBooking>()
                .HasOne(b => b.Registration)
                .WithMany()
                .HasForeignKey(b => b.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => (x.Entity is Conference || x.Entity is Models.Delegate1 ||
                             x.Entity is Session || x.Entity is Registration ||
                             x.Entity is RestStop || x.Entity is RestStopBooking ||
                             x.Entity is ApplicationUser) &&
                            (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;

                if (entity.State == EntityState.Added)
                {
                    entity.Property("CreatedAt").CurrentValue = now;
                }

                if (entity.Property("UpdatedAt") != null)
                {
                    entity.Property("UpdatedAt").CurrentValue = now;
                }
            }
        }
    }
}