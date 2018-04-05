using ProjectAIP.BLL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ProjectAIP.DAL.EF
{
    public static class Seed
    {
        public static void RunSeed(DbContext context, RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            // Seed operations
        }

    }
}
