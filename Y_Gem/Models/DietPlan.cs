namespace Y_GYM.Models
{
    public class DietPlan
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int CoachId { get; set; }
        public Coach Coach { get; set; }
        public string? PlanDetails { get; set; }
        public DateTime StartDate { get; set; }

    }
}
