using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;
using SystemMagazynu.Models;
using SystemMagazynu.ViewModels;

namespace SystemMagazynu.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var model = new DashboardViewModel
            {
                ProductsCount = await _db.Products.CountAsync(p => p.IsActive),
                WarehousesCount = await _db.Warehouse.CountAsync(w => w.IsActive),
                SuppliersCount = await _db.Suppliers.CountAsync(s => s.IsActive),
                ActiveAlertsCount = await _db.StockAlerts.CountAsync(a => a.Status == AlertStatus.Active),
                LowStockPositions = await _db.WarehouseStocks.CountAsync(s => s.Quantity <= s.Product!.MinimumStock),
                DeliveriesThisMonth = await _db.Deliveries.CountAsync(d => d.DeliveryDate >= monthStart),
                IssuesThisMonth = await _db.WarehouseMovements
                    .CountAsync(m => m.MovementType == MovementType.Issue && m.MovementDate >= monthStart),

                RecentDeliveries = await _db.Deliveries
                    .Include(d => d.Supplier)
                    .Include(d => d.Warehouse)
                    .OrderByDescending(d => d.DeliveryDate)
                    .Take(5)
                    .ToListAsync(),

                RecentAlerts = await _db.StockAlerts
                    .Include(a => a.Product)
                    .Include(a => a.Warehouse)
                    .Where(a => a.Status == AlertStatus.Active)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
