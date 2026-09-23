using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Y_GYM.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;

        // Membership plan is optional when creating
        // the member account.
        public int? MembershipPlanId { get; set; }

        [ForeignKey("MembershipPlanId")]
        public MembershipPlan? Plan { get; set; }

        public double? Weight { get; set; }

        public double? Height { get; set; }

        public string? Goal { get; set; }

        public string? FitnessLevel { get; set; }

        public string? QRCode { get; set; }

        // Link between Member and ApplicationUser
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<CheckIn>? CheckIns { get; set; }

        public ICollection<Subscription>? Subscriptions { get; set; }

        public ICollection<Booking>? Booking { get; set; }
    }
}