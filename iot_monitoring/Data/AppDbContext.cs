using iot_monitoring.Models;
using Microsoft.EntityFrameworkCore;
namespace iot_monitoring.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users{ get; set; }
        public DbSet<Device> Devices { get; set; }
    }
}