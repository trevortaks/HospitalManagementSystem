using HospitalManagementSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Infrastructure.Data;

/// <summary>
/// Seeds initial data for the application, including a default super administrator user.
/// </summary>
public class DatabaseInitializer
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DatabaseInitializer(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Ensures that essential roles and the default super administrator user exist.
    /// </summary>
    public async Task InitializeAsync()
    {
        const string adminRole = "Administrator";
        const string adminUser = "superadmin";
        const string adminEmail = "superadmin@example.com";
        const string adminPassword = "SuperAdmin123!";

        if (!await _roleManager.RoleExistsAsync(adminRole))
        {
            var role = new ApplicationRole { Name = adminRole, Description = "System administrator" };
            await _roleManager.CreateAsync(role);
        }

        var user = await _userManager.FindByNameAsync(adminUser);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminUser,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(user, adminPassword);
            await _userManager.AddToRoleAsync(user, adminRole);
        }
    }
}
