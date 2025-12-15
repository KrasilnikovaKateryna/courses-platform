using CoursePlatform.Models;
using Microsoft.AspNetCore.Identity;

namespace CoursePlatform.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Admin", "Teacher", "Student"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, "admin@demo.com", "Test@123JesusGayHrenkAtIA*bi", "Admin", "Admin User");
        await EnsureUserAsync(userManager, "teacher@demo.com", "Test@123JesusGayHrenkAtIA*bi", "Teacher", "Teacher User");
        await EnsureUserAsync(userManager, "student@demo.com", "Test@123JesusGayHrenkAtIA*bi", "Student", "Student User");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string fullName)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }
        else
        {
            user.FullName = fullName;
            await userManager.UpdateAsync(user);

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, password);
            if (!resetResult.Succeeded)
                throw new Exception(string.Join("; ", resetResult.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }

}