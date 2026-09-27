using System.ComponentModel.DataAnnotations;

namespace Y_GYM.Models.ViewModels
{
    public class RegisterMemberVM
    {
        // =========================================================
        // ACCOUNT INFORMATION
        // =========================================================

        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string UserName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(
            6,
            ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        // =========================================================
        // FITNESS INFORMATION
        // =========================================================

        [Display(Name = "Weight")]
        [Range(
            1,
            500,
            ErrorMessage = "Please enter a valid weight.")]
        public double? Weight { get; set; }


        [Display(Name = "Height")]
        [Range(
            1,
            300,
            ErrorMessage = "Please enter a valid height.")]
        public double? Height { get; set; }


        [Display(Name = "Goal")]
        public string? Goal { get; set; }


        [Display(Name = "Fitness Level")]
        public string? FitnessLevel { get; set; }
    }
}