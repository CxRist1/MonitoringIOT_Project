using iot_monitoring.Data;
using iot_monitoring.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace iot_monitoring.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);
            var sevenDaysAgoUtc = todayUtc.AddDays(-6);

            var todayRevenue = await _context.Payments
                .AsNoTracking()
                .Where(p =>
                    p.Status == "Paid" &&
                    p.PaidAt.HasValue &&
                    p.PaidAt.Value >= todayUtc &&
                    p.PaidAt.Value < tomorrowUtc)
                .SumAsync(p => (decimal?)p.Amount)
                ?? 0m;
            Console.WriteLine($"Today Revenue = {todayRevenue}");

            var todayOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o =>
                o.CreatedAt >= todayUtc &&
                o.CreatedAt < tomorrowUtc);

            var pendingOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o =>
                    o.Status == "PendingPayment");

            var completedOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o =>
                o.Status == "Completed");

            var totalUser = await _context.Users
                .AsNoTracking()
                .CountAsync();

            var totalProducts = await _context.Products
                .AsNoTracking()
                .CountAsync();

            var revenueByDay = await _context.Payments
                .AsNoTracking()
                .Where(p =>
                    p.Status == "Paid" &&
                    p.PaidAt.HasValue &&
                    p.PaidAt.Value >= sevenDaysAgoUtc &&
                    p.PaidAt.Value < tomorrowUtc)
                .GroupBy(p => p.PaidAt!.Value.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Revenue = group.Sum(p => p.Amount)
                })
                .ToListAsync();
            Console.WriteLine($"Revenue Rows = {revenueByDay.Count}");

            foreach (var item in revenueByDay)
            {
                Console.WriteLine($"{item.Date} : {item.Revenue}");
            }

            var revenueLabels = new List<string>();
            var revenueData = new List<decimal>();

            for (var date = sevenDaysAgoUtc; date <= todayUtc; date = date.AddDays(1))
            {
                var dailyRevenue = revenueByDay
                    .FirstOrDefault(item => item.Date == date)
                    ?.Revenue ?? 0m;

                revenueLabels.Add(date.ToString("dd MMM"));
                revenueData.Add(dailyRevenue);
            }

            var model = new AdminDashboardViewModel
            {
                TodayRevenue = todayRevenue,
                TodayOrders = todayOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                TotalUsers = totalUser,
                TotalProducts = totalProducts,
                RevenueLabels = revenueLabels,
                RevenueData = revenueData
            };

            return View(model);
        }
    }
}
