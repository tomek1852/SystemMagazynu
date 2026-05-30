namespace SystemMagazynu.Models
{
    public enum MovementType
    {
        Receipt,
        Issue,
        Correction
    }
    public class WarehouseMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public MovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
        public string? SourceDocument { get; set; }
        public string? Notes { get; set; }
        public string? UserId { get; set; }
        public Product? Product { get; set; }
        public Warehouse? Warehouse { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
