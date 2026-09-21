using Microsoft.EntityFrameworkCore;
using Y_Gem.Data;
using Y_Gem.Models;

namespace Y_Gem.Repository
{
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(ApplicationDbContext context) : base(context) { }

        public  Member GetByUserId(string userId)
            =>  _dbSet.Include(m => m.User)
                            .FirstOrDefault(m => m.UserId == userId);

        public  Member GetWithDetails(int memberId)
            =>  _dbSet
                .Include(m => m.User)
                .Include(m => m.Subscriptions).ThenInclude(s => s.MembershipPlan)
                .Include(m => m.Bookings).ThenInclude(b => b.ClassSchedule).ThenInclude(cs => cs.Classe)
                .FirstOrDefault(m => m.Id == memberId);

        public Subscription GetActiveSubscription(int memberId)
            =>  _context.Subscriptions
                .Include(s => s.MembershipPlan)
                .Where(s => s.MemberId == memberId && s.EndDate >= DateTime.Now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

        public  int GetAttendanceCountThisMonth(int memberId)
        {
            var now = DateTime.Now;
            return  _context.CheckIns
                .Count(c => c.MemberId == memberId
                    && c.CheckInTime.Month == now.Month
                    && c.CheckInTime.Year == now.Year
                    && c.Status == "Allowed");
        }
    }
}