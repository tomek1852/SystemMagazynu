using Microsoft.AspNetCore.Identity;
using SystemMagazynu.Models;

namespace SystemMagazynu.Data
{
    public static class DbSeeder
    {
        public static readonly string[] Roles = new[]
        {
            "Administrator",
            "Magazynier",
            "KierownikMagazynu",
            "PracownikZakupow",
            "UzytkownikOdczytu"
        };

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            const string adminEmail = "admin@warehouse.pl";
            const string adminPassword = "Admin1234!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Administrator",
                    LastName = "Systemu",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }
    }
}
