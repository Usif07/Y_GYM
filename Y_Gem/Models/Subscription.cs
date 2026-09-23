using System.ComponentModel.DataAnnotations;

namespace Y_GYM.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a member")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        public Member? Member { get; set; }


        [Required(ErrorMessage = "Please select a membership plan")]
        [Display(Name = "Membership Plan")]
        public int PlanId { get; set; }

        public MembershipPlan? MembershipPlan { get; set; }


        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;


        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }


        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }
}