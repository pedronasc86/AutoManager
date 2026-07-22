using Microsoft.AspNetCore.Identity;

namespace Indentity.API.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string? name { get; set; }
    }
}
