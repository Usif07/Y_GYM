using Y_GYM.Models;

namespace Y_GYM.Repository
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
        Member? GetByUserId(string userId);

        Member? GetWithDetails(int memberId);

        Subscription? GetActiveSubscription(int memberId);

        int GetAttendanceCountThisMonth(int memberId);
    }
}
