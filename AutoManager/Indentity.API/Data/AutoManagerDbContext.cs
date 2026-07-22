using Indentity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Indentity.API.Data
{
    public class AutoManagerDbContext : IdentityDbContext<ApplicationUser>
    {
        public AutoManagerDbContext(DbContextOptions<AutoManagerDbContext> options) : base(options) { }
    }
}
