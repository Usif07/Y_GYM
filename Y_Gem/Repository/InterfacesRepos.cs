using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Repository
{
    public class CoachRepository : GenericRepository<Coach>, ICoachRepository
    {
        public CoachRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    public class CheckInRepository : GenericRepository<CheckIn>, ICheckInRepository
    {
        public CheckInRepository(ApplicationDbContext context) : base(context) { }
        public IEnumerable<CheckIn> GetAllWithDetails() => _context.Set<CheckIn>().Include(c => c.Member).ToList();
    }
 
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }
        public IEnumerable<Payment> GetAllWithDetails() => _context.Set<Payment>().Include(p => p.Member).ToList();
    }

    public class ProgressRepository : GenericRepository<Progress>, IProgressRepository
    {
        public ProgressRepository(ApplicationDbContext context) : base(context) { }
        public IEnumerable<Progress> GetAllWithDetails() => _context.Set<Progress>().Include(p => p.Member).ToList();
    }

    public class ClassScheduleRepository : GenericRepository<ClassSchedule>, IClassScheduleRepository
    {
        public ClassScheduleRepository(ApplicationDbContext context) : base(context) { }
        // Schedule مربوط بالكلاس والمدرب
        public IEnumerable<ClassSchedule> GetAllWithDetails() =>
            _context.Set<ClassSchedule>().Include(c => c.Class).Include(c => c.Coach).ToList();
    }

    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context) { }
        public IEnumerable<Booking> GetAllWithDetails() =>
            _context.Set<Booking>().Include(b => b.Member).Include(b => b.ClassSchedule).ToList();
    }

    public class DietPlanRepository : GenericRepository<DietPlan>, IDietPlanRepository
    {
        public DietPlanRepository(ApplicationDbContext context) : base(context) { }
        public IEnumerable<DietPlan> GetAllWithDetails() =>
            _context.Set<DietPlan>().Include(d => d.Member).Include(d => d.Coach).ToList();
    }

    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        public ClassRepository(ApplicationDbContext context) : base(context) { }
    }
    public class AdminRepository : GenericRepository<Admin>, IAdminRepository
    {
        public AdminRepository(ApplicationDbContext context) : base(context) { }
    }
    public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(ApplicationDbContext context) : base(context) { }
    }

    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context) { }

        // الاشتراك مربوط بالعضو وخطة الاشتراك
        public IEnumerable<Subscription> GetAllWithDetails() =>
            _context.Set<Subscription>()
            .Include(s => s.Member)
            .Include(s => s.MembershipPlan)
            .ToList();
    }
}