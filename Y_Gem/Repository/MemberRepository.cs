using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;

namespace Y_GYM.Repository
{
    public class MemberRepository
        : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public Member? GetByUserId(string userId)
        {
            return _dbSet
                .Include(m => m.User)
                .Include(m => m.Plan)
                .FirstOrDefault(m => m.UserId == userId);
        }

        public Member? GetWithDetails(int memberId)
        {
            return _dbSet
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Include(m => m.Subscriptions)
                    .ThenInclude(s => s.MembershipPlan)
                .Include(m => m.Booking)
                    .ThenInclude(b => b.ClassSchedule)
                        .ThenInclude(cs => cs.Class)
                .Include(m => m.Booking)
                    .ThenInclude(b => b.ClassSchedule)
                        .ThenInclude(cs => cs.Coach)
                            .ThenInclude(c => c.User)
                .FirstOrDefault(m => m.Id == memberId);
        }

        public Subscription? GetActiveSubscription(int memberId)
        {
            return _context.Subscriptions
                .Include(s => s.MembershipPlan)
                .Where(s =>
                    s.MemberId == memberId &&
                    s.EndDate >= DateTime.Now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();
        }

        public int GetAttendanceCountThisMonth(int memberId)
        {
            var now = DateTime.Now;

            return _context.CheckIns
                .Count(c =>
                    c.MemberId == memberId &&
                    c.CheckInTime.Month == now.Month &&
                    c.CheckInTime.Year == now.Year &&
                    c.Status == "Allowed");
        }
    }
}