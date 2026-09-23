using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Models;

namespace Y_GYM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // Gym Members & Staff
        public DbSet<Member> Members { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Coach> Coaches { get; set; }

        // Membership
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Classes & Bookings
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // Attendance
        public DbSet<CheckIn> CheckIns { get; set; }

        // Diet & Progress
        public DbSet<DietPlan> DietPlans { get; set; }
        public DbSet<Progress> ProgressLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================================================
            // MEMBER → APPLICATION USER
            // =========================================================

            builder.Entity<Member>()
                .HasOne(m => m.User)
                .WithOne()
                .HasForeignKey<Member>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // STAFF → APPLICATION USER
            // =========================================================

            builder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                 .Property(p => p.Amount)
                 .HasPrecision(18, 2);


            // =========================================================
            // COACH → APPLICATION USER
            // =========================================================

            builder.Entity<Coach>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Coach>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // SUBSCRIPTION → MEMBERSHIP PLAN
            // =========================================================

            builder.Entity<Subscription>()
                .HasOne(s => s.MembershipPlan)
                .WithMany()
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // BOOKING → CLASS SCHEDULE
            // =========================================================

            builder.Entity<Booking>()
                .HasOne(b => b.ClassSchedule)
                .WithMany()
                .HasForeignKey(b => b.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // CHECK-IN → MEMBER
            // =========================================================

            builder.Entity<CheckIn>()
                .HasOne(c => c.Member)
                .WithMany(m => m.CheckIns)
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // CHECK-IN → STAFF
            // =========================================================

            builder.Entity<CheckIn>()
                .HasOne(c => c.Staff)
                .WithMany(s => s.CheckIns)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // CLASS SCHEDULE → CLASS
            // =========================================================

            builder.Entity<ClassSchedule>()
                .HasOne(cs => cs.Class)
                .WithMany()
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // CLASS SCHEDULE → COACH
            // =========================================================

            builder.Entity<ClassSchedule>()
                .HasOne(cs => cs.Coach)
                .WithMany()
                .HasForeignKey(cs => cs.CoachId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // DIET PLAN → COACH
            // =========================================================

            builder.Entity<DietPlan>()
                .HasOne(d => d.Coach)
                .WithMany()
                .HasForeignKey(d => d.CoachId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}