using System.Numerics;

namespace Y_Gem.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int PlanId { get; set; }
        public MembershipPlan MembershipPlan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

    }
}
