using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Data;
using SystemMagazynu.Models;

namespace SystemMagazynu.Services
{
    public class WarehouseStockService : IWarehouseStockService
    {
        private readonly ApplicationDbContext _db;
        private readonly IStockAlertService _alertService;

        public WarehouseStockService(ApplicationDbContext db, IStockAlertService alertService)
        {
            _db = db;
            _alertService = alertService;
        }

        public async Task<WarehouseStock?> GetStockAsync(int productId, int warehouseId)
        {
            return await _db.WarehouseStocks
                .FirstOrDefaultAsync(s => s.ProductId == productId
                                       && s.WarehouseId == warehouseId);
        }

        public async Task IncreaseStockAsync(int productId, int warehouseId, int quantity,
                                              string userId, string? sourceDocument = null)
        {
            var stock = await GetOrCreateStockAsync(productId, warehouseId);

            stock.Quantity += quantity;
            stock.UpdatedAt = DateTime.UtcNow;

            _db.WarehouseMovements.Add(new WarehouseMovement
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                MovementType = MovementType.Receipt,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                SourceDocument = sourceDocument,
                UserId = userId
            });

            await _db.SaveChangesAsync();
            await _alertService.CheckAndCreateAlertsAsync(productId, warehouseId);
        }

        public async Task<bool> DecreaseStockAsync(int productId, int warehouseId, int quantity,
                                                    string userId, string? sourceDocument = null)
        {
            var stock = await GetOrCreateStockAsync(productId, warehouseId);

            if (stock.Quantity < quantity)
                return false;

            stock.Quantity -= quantity;
            stock.UpdatedAt = DateTime.UtcNow;

            _db.WarehouseMovements.Add(new WarehouseMovement
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                MovementType = MovementType.Issue,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                SourceDocument = sourceDocument,
                UserId = userId
            });

            await _db.SaveChangesAsync();
            await _alertService.CheckAndCreateAlertsAsync(productId, warehouseId);
            return true;
        }

        public async Task CorrectStockAsync(int productId, int warehouseId, int newQuantity,
                                             string userId, string? notes = null)
        {
            var stock = await GetOrCreateStockAsync(productId, warehouseId);
            int oldQuantity = stock.Quantity;

            stock.Quantity = newQuantity;
            stock.UpdatedAt = DateTime.UtcNow;

            _db.WarehouseMovements.Add(new WarehouseMovement
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                MovementType = MovementType.Correction,
                Quantity = newQuantity - oldQuantity,
                MovementDate = DateTime.UtcNow,
                Notes = notes ?? $"Korekta stanu: {oldQuantity} → {newQuantity}",
                UserId = userId
            });

            await _db.SaveChangesAsync();
            await _alertService.CheckAndCreateAlertsAsync(productId, warehouseId);
        }

        private async Task<WarehouseStock> GetOrCreateStockAsync(int productId, int warehouseId)
        {
            var stock = await _db.WarehouseStocks
                .FirstOrDefaultAsync(s => s.ProductId == productId
                                       && s.WarehouseId == warehouseId);

            if (stock == null)
            {
                stock = new WarehouseStock
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.WarehouseStocks.Add(stock);
            }

            return stock;
        }
    }
}