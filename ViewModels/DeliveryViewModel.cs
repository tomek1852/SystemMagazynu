using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class DeliveryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dostawca jest wymagany.")]
        [Display(Name = "Dostawca")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Magazyn jest wymagany.")]
        [Display(Name = "Magazyn")]
        public int WarehouseId { get; set; }

        [Required(ErrorMessage = "Numer dostawy jest wymagany.")]
        [MaxLength(50, ErrorMessage = "Numer dostawy może mieć maksymalnie 50 znaków.")]
        [Display(Name = "Numer dostawy")]
        public string DeliveryNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data dostawy jest wymagana.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data dostawy")]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [MaxLength(1000, ErrorMessage = "Uwagi mogą mieć maksymalnie 1000 znaków.")]
        [Display(Name = "Uwagi")]
        public string? Notes { get; set; }

        public List<DeliveryItemViewModel> Items { get; set; } = new();
    }

    public class DeliveryItemViewModel
    {
        [Required(ErrorMessage = "Produkt jest wymagany.")]
        [Display(Name = "Produkt")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od zera.")]
        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cena nie może być ujemna.")]
        [Display(Name = "Cena jednostkowa")]
        public decimal UnitPrice { get; set; }
    }
}
