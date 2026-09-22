using Microsoft.AspNetCore.Identity;

namespace Y_GYM.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}