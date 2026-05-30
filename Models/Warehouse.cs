namespace SystemMagazynu.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set;  } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public ICollection<WarehouseMovement> WarehouseMovements { get; set;} = new List<WarehouseMovement>();
        public ICollection<StockAlert> StockAlerts { get; set;} = new List<StockAlert>();
    }
}
