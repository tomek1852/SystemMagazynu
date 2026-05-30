namespace SystemMagazynu.Models
{
    public class WarehouseStock
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Warehouse? Warehouse { get; set; }
        public Product? Product { get; set; }
    }
}
