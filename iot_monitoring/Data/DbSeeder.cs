using iot_monitoring.Models;
using iot_monitoring.Services;
namespace iot_monitoring.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(AppDbContext context, PasswordService passwordService)
        {
            if (context.Users.Any())
            {
                return;
            }
            var admin = new User
            {
                Username = "admin",
                Password = passwordService.HashPassword("1234"),
                FullName = "Administrator",
                Email = "admin@iot.local",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now,
            };
            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
