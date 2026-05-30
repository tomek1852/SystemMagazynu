namespace SystemMagazynu.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public string DeliveryNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Supplier? Supplier { get; set; }
        public Warehouse? Warehouse { get; set; }
        public ApplicationUser? User { get; set; }
        public ICollection<DeliveryItem> DeliveryItems { get; set; } = new List<DeliveryItem>();
    }
}
