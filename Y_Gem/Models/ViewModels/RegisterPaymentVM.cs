using System.ComponentModel.DataAnnotations;

namespace Y_GYM.Models.ViewModels
{
    public class RegisterPaymentVM
    {
        [Required(ErrorMessage = "Please enter the member phone number.")]
        [Display(Name = "Member Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a membership plan.")]
        [Display(Name = "Membership Plan")]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "Please enter the amount paid.")]
        [Range(1, 1000000, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }
    }
}