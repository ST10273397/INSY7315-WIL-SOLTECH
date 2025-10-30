using ElevatedTutors.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ElevatedTutors.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        // --- ADD the domain DbSets previously in AppDbContext (from INSY) ---
        public DbSet<StudentUser>? StudentUsers { get; set; }
        public DbSet<TutorUser>? TutorUsers { get; set; }
        public DbSet<AdminUser>? AdminUsers { get; set; }
        public DbSet<Subject>? Subjects { get; set; }
        public DbSet<Session>? Sessions { get; set; }
        public DbSet<Submission>? Submissions { get; set; }
        public DbSet<SubmissionFile>? SubmissionFiles { get; set; }


        // If you used custom OnModelCreating in INSY, merge it here:
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ----------------------------
            // One-to-one relationships with User
            // ----------------------------
           builder.Entity<StudentUser>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<StudentUser>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TutorUser>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<TutorUser>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AdminUser>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<AdminUser>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------------------
            // Decimal precision for grades/marks
            // ----------------------------
            builder.Entity<StudentUser>()
                .Property(s => s.Marks)
                .HasPrecision(5, 2);

            builder.Entity<Submission>()
                .Property(s => s.Grade)
                .HasPrecision(5, 2);

            // ----------------------------
            // Subjects 
            // ----------------------------
            builder.Entity<Subject>()
                .HasOne(s => s.TutorUser)
                .WithMany(t => t.Subjects)
                .HasForeignKey(s => s.TutorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Subject>()
                .HasOne(s => s.StudentUser)
                .WithMany(su => su.Subjects)
                .HasForeignKey(s => s.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------
            // Sessions
            // ----------------------------
            builder.Entity<Session>()
                .HasOne(s => s.StudentUser)
                .WithMany(su => su.Sessions)
                .HasForeignKey(s => s.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Session>()
                .HasOne(s => s.TutorUser)
                .WithMany(tu => tu.Sessions)
                .HasForeignKey(s => s.TutorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Session>()
                .HasOne(s => s.Subject)
                .WithMany(sub => sub.Sessions)
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------
            // Submissions
            // ----------------------------
            builder.Entity<Submission>()
                .HasOne(s => s.Subject)
                .WithMany(sub => sub.Submissions)
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Submission>()
                .HasOne(s => s.StudentUser)
                .WithMany(su => su.Submissions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------
            // SubmissionFiles (One Submission -> Many Files)
            // ----------------------------
            builder.Entity<SubmissionFile>()
                .HasOne(sf => sf.Submission)
                .WithMany(s => s.Files) // Collection in Submission
                .HasForeignKey(sf => sf.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict); // if a submission is deleted, remove its files


            // ----------------------------
            // Configure indexes for performance
            // ----------------------------
            builder.Entity<Session>().HasIndex(s => s.StudentUserId);
            builder.Entity<Session>().HasIndex(s => s.TutorUserId);
            builder.Entity<Session>().HasIndex(s => s.SubjectId);

            builder.Entity<Submission>().HasIndex(s => s.UserId);
            builder.Entity<Submission>().HasIndex(s => s.SubjectId);

            builder.Entity<Subject>().HasIndex(s => s.TutorUserId);
            builder.Entity<Subject>().HasIndex(s => s.StudentUserId);
        }
    }
}
