using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Y_GYM.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string ShiftTime { get; set; } = string.Empty;

        public ICollection<CheckIn> CheckIns { get; set; }
            = new List<CheckIn>();

        public ICollection<Payment> PaymentsHandled { get; set; }
            = new List<Payment>();
    }
}