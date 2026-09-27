using System.ComponentModel.DataAnnotations;

namespace Y_GYM.Models.ViewModels
{
    public class CheckInVM
    {
        [Required(ErrorMessage = "Please enter the member phone number.")]
        [Display(Name = "Member Phone")]
        public string Phone { get; set; } = string.Empty;
    }
}