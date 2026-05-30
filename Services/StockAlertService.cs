using SystemMagazynu.Data;
using SystemMagazynu.Models;
using Microsoft.EntityFrameworkCore;

namespace SystemMagazynu.Services
{
    public class StockAlertService : IStockAlertService
    {
        private readonly ApplicationDbContext _db;

        public StockAlertService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CheckAndCreateAlertsAsync(int productId, int warehouseId)
        {
            var stock = await _db.WarehouseStocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == productId
                                       && s.WarehouseId == warehouseId);

            if (stock?.Product == null) return;

            bool isBelowMin = stock.Quantity <= stock.Product.MinimumStock;

            var existingAlert = await _db.StockAlerts
                .FirstOrDefaultAsync(a => a.ProductId == productId
                                       && a.WarehouseId == warehouseId
                                       && a.Status == AlertStatus.Active);

            if (isBelowMin && existingAlert == null)
            {
                _db.StockAlerts.Add(new StockAlert
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    CurrentQuantity = stock.Quantity,
                    MinimumStock = stock.Product.MinimumStock,
                    Status = AlertStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
            }
            else if (!isBelowMin && existingAlert != null)
            {
                existingAlert.Status = AlertStatus.Resolved;
                existingAlert.ResolvedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task ResolveAlertAsync(int alertId)
        {
            var alert = await _db.StockAlerts.FindAsync(alertId);
            if (alert == null) return;

            alert.Status = AlertStatus.Resolved;
            alert.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
