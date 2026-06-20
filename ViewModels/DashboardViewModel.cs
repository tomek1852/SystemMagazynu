using SystemMagazynu.Models;

namespace SystemMagazynu.ViewModels
{
    public class DashboardViewModel
    {
        public int ProductsCount { get; set; }
        public int WarehousesCount { get; set; }
        public int SuppliersCount { get; set; }
        public int ActiveAlertsCount { get; set; }
        public int LowStockPositions { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int IssuesThisMonth { get; set; }

        public List<Delivery> RecentDeliveries { get; set; } = new();
        public List<StockAlert> RecentAlerts { get; set; } = new();
    }
}
