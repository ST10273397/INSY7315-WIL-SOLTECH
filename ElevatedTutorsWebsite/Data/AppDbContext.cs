using ElevatedTutorsWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace ElevatedTutorsWebsite.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<StudentUser> StudentUsers { get; set; }
        public DbSet<TutorUser> TutorUsers { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------
            // One-to-one relationships with User
            // ----------------------------
            modelBuilder.Entity<StudentUser>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<StudentUser>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TutorUser>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<TutorUser>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdminUser>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<AdminUser>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------------------
            // Decimal precision for grades/marks
            // ----------------------------
            modelBuilder.Entity<StudentUser>()
                .Property(s => s.Marks)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Submission>()
                .Property(s => s.Grade)
                .HasPrecision(5, 2);

            // ----------------------------
            // Subjects 
            // ----------------------------
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.TutorUser)
                .WithMany(t => t.Subjects)
                .HasForeignKey(s => s.TutorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subject>()
                .HasOne(s => s.StudentUser)
                .WithMany(su => su.Subjects)
                .HasForeignKey(s => s.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------
            // Sessions
            // ----------------------------
            modelBuilder.Entity<Session>()
                .HasOne(s => s.StudentUser)
                .WithMany(su => su.Sessions)
                .HasForeignKey(s => s.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.TutorUser)
                .WithMany(tu => tu.Sessions)
                .HasForeignKey(s => s.TutorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Subject)
                .WithMany(sub => sub.Sessions)
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------
            // Submissions
            // ----------------------------
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Subject)
                .WithMany(sub => sub.Submissions)
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.StudentUser)
                .WithMany(su => su.Submissions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------
            // Configure indexes for performance
            // ----------------------------
            modelBuilder.Entity<Session>().HasIndex(s => s.StudentUserId);
            modelBuilder.Entity<Session>().HasIndex(s => s.TutorUserId);
            modelBuilder.Entity<Session>().HasIndex(s => s.SubjectId);

            modelBuilder.Entity<Submission>().HasIndex(s => s.UserId);
            modelBuilder.Entity<Submission>().HasIndex(s => s.SubjectId);

            modelBuilder.Entity<Subject>().HasIndex(s => s.TutorUserId);
            modelBuilder.Entity<Subject>().HasIndex(s => s.StudentUserId);
        }

    }
}
