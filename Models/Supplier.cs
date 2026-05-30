namespace SystemMagazynu.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NIP { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Street { get; set; }
        public string? BuildingNumber { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}
