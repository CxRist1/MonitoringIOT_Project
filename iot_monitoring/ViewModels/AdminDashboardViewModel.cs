namespace iot_monitoring.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalUsers { get; set; }
        public int CompletedOrders { get; set; }
        public int TotalProducts { get; set; }
        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();
    }
}
