using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Y_GYM.Models
{
    public class CheckIn
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a member")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }

        [Required(ErrorMessage = "Please select a staff member")]
        [Display(Name = "Staff")]
        public int StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff? Staff { get; set; }

        [Required]
        [Display(Name = "Check In Time")]
        public DateTime CheckInTime { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Allowed";
    }
}