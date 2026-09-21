// Repository/IMemberRepository.cs
using Y_Gem.Models;

namespace Y_Gem.Repository
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
        Member GetByUserId(string userId);
        Member GetWithDetails(int memberId);   // مع Subscription/Bookings
        Subscription GetActiveSubscription(int memberId);
        int GetAttendanceCountThisMonth(int memberId);
    }
}