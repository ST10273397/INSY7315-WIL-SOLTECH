using ElevatedTutors.Models;
using Microsoft.AspNetCore.Identity;

namespace ElevatedTutors.Data
{
    public static class SeedData
    {
        public static async Task CreateRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Admin", "Tutor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create admin user if it doesn't exist
            var adminEmail = "admin@local.test";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin@local.test",
                    Email = adminEmail,
                    FirstName = "System",
                    Surname = "Admin",
                    Role = "Admin",
                    EmailConfirmed = true,
                    IsApproved = true
                };
                await userManager.CreateAsync(admin, "Admin123!");

            }
        }
    }
}
