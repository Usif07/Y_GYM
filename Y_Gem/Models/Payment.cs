using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Y_GYM.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 10000)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public int MemberId { get; set; }
        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        // ================= الخصائص اللي كانت ناقصة وعاملة إيرور =================

        public int? SubscriptionId { get; set; }
        [ForeignKey("SubscriptionId")]
        public Subscription? Subscription { get; set; }

        public int? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff? Staff { get; set; }
    }
}