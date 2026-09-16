using Microsoft.EntityFrameworkCore;
using Y_Gem.Models;

namespace Y_Gem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Classe> Classes { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<DietPlan> DietPlans { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Progress> Progresses { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Coach>().ToTable("Coaches");
            modelBuilder.Entity<Member>().ToTable("Members");
            modelBuilder.Entity<Staff>().ToTable("Staffs");

           
            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.Staff)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.Member)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

         
            modelBuilder.Entity<DietPlan>()
                .HasOne(d => d.Member)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DietPlan>()
                .HasOne(d => d.Coach)
                .WithMany(c => c.DietPlans)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassSchedule>()
                .HasOne(cs => cs.Coach)
                .WithMany(c => c.ClassSchedules)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Member)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}