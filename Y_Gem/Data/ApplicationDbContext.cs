// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Models;
using Microsoft.AspNetCore.Identity;


namespace Y_GYM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Member> Members { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<DietPlan> DietPlans { get; set; }
        public DbSet<Progress> ProgressLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Member <-> ApplicationUser (1-to-1)
            builder.Entity<Member>()
                .HasOne(m => m.User)
                .WithOne()
                .HasForeignKey<Member>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Staff <-> ApplicationUser (1-to-1)
            builder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Subscription>()
                .HasOne(s => s.MembershipPlan)
                .WithMany()
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.ClassSchedule)
                .WithMany()
                .HasForeignKey(b => b.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);


            // منع Cascade Delete المتعدد المسارات في CheckIn
            builder.Entity<CheckIn>()
                .HasOne(c => c.Member)
                .WithMany(m => m.CheckIns)
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CheckIn>()
                .HasOne(c => c.Staff)
                .WithMany(s => s.CheckIns)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassSchedule>()
                .HasOne(cs => cs.Coach)
                .WithMany()
                .HasForeignKey("CoachId1")     // العمود الـ string الحقيقي
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassSchedule>()
                .HasOne(cs => cs.Class)
                .WithMany()
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
        }



    }
}