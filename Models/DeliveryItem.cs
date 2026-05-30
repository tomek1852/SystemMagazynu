namespace SystemMagazynu.Models
{
    public class DeliveryItem
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public Delivery? Delivery { get; set; }
        public Product? Product { get; set; }
    }
}
