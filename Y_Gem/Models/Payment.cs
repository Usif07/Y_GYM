using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Y_GYM.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(typeof(decimal), "1", "1000000",
            ErrorMessage = "Amount must be greater than zero")]
        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // =========================
        // Member
        // =========================

        [Required(ErrorMessage = "Please select a member")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }

        // =========================
        // Subscription
        // =========================

        [Display(Name = "Subscription")]
        public int? SubscriptionId { get; set; }

        [ForeignKey(nameof(SubscriptionId))]
        public Subscription? Subscription { get; set; }

        // =========================
        // Staff
        // =========================

        [Display(Name = "Staff")]
        public int? StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff? Staff { get; set; }
    }
}