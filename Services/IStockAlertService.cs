namespace SystemMagazynu.Services
{
    public interface IStockAlertService
    {
        Task CheckAndCreateAlertsAsync(int productId, int warehouseId);
        Task ResolveAlertAsync(int alertId);
    }
}
