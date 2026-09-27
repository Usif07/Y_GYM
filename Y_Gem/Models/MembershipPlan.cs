using System.ComponentModel.DataAnnotations;

namespace Y_GYM.Models
{
    public class MembershipPlan
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [Range(1, 3650)]
        public int DurationDays { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Total Visits")]
        public int TotalVisits { get; set; }
    }
}