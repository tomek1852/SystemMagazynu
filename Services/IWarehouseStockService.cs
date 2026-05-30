using SystemMagazynu.Models;

namespace SystemMagazynu.Services
{
    public interface IWarehouseStockService
    {
        Task<WarehouseStock?> GetStockAsync(int productId, int warehouseId);
        Task IncreaseStockAsync(int productId, int warehouseId, int quantity,
                                string userId, string? sourceDocument = null);
        Task<bool> DecreaseStockAsync(int productId, int warehouseId, int quantity,
                                      string userId, string? sourceDocument = null);
        Task CorrectStockAsync(int productId, int warehouseId, int newQuantity,
                               string userId, string? notes = null);
    }
}