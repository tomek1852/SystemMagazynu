namespace SystemMagazynu.Models
{
    public enum AlertStatus
    {
        Active,
        Resolved
    }
    public class StockAlert
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumStock { get; set; }
        public AlertStatus Status { get; set; } = AlertStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        public Product? Product { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
