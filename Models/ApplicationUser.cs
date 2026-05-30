using Microsoft.AspNetCore.Identity;

namespace SystemMagazynu.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string FullName => $"{FirstName} {LastName}";

        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public ICollection<WarehouseMovement> WarehouseMovements { get; set; } = new List<WarehouseMovement>();
        public ICollection<ChangeHistory> ChangeHistories { get; set; } = new List<ChangeHistory>();
    }
}
