namespace SystemMagazynu.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CatalogNumber { get; set; } = string.Empty;
        public string? Description {  get; set; }
        public int MinimumStock { get; set; }
        public bool IsActive { get; set; } = true;

        public Category? Category { get; set; }
        public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
        public ICollection<DeliveryItem> DeliveryItems { get; set; } = new List<DeliveryItem>();
        public ICollection<WarehouseMovement> WarehouseMovements { get; set; } = new List<WarehouseMovement>();
        public ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();
    }
}
