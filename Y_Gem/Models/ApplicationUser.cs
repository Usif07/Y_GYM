using Microsoft.AspNetCore.Identity;

namespace Y_Gem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}