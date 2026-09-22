using System.ComponentModel.DataAnnotations;

namespace Y_GYM.Models.ViewModels
{
    public class RegisterMemberVM
    {
        [Required(ErrorMessage = "Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "The email address is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number required")]
        [Phone(ErrorMessage = "The phone number is incorrect")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password Required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Range(30, 300, ErrorMessage = "The weight doesn't make sense")]
        public double Weight { get; set; }

        [Range(100, 250, ErrorMessage = "The height doesn't make sense")]
        public double Height { get; set; }

        [Required]
        public string Goal { get; set; }

        [Required]
        public string FitnessLevel { get; set; }
    }
}