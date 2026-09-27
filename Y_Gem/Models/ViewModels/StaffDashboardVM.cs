using Y_GYM.Models;

namespace Y_GYM.Models.ViewModels
{
    public class StaffDashboardVM
    {
        public Staff? Staff { get; set; }

        public bool IsAdmin { get; set; }

        public int TotalMembers { get; set; }

        public int ActiveMembers { get; set; }

        public int TodayCheckIns { get; set; }

        public decimal TodayPayments { get; set; }

        public List<CheckIn> RecentCheckIns { get; set; }
            = new();

        public List<Payment> RecentPayments { get; set; }
            = new();
    }
}