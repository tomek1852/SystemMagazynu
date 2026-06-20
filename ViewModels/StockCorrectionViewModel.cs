using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class StockCorrectionViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        // Pola tylko do wyświetlenia
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Nowy stan nie może być ujemny.")]
        [Display(Name = "Nowy stan")]
        public int NewQuantity { get; set; }

        [MaxLength(500, ErrorMessage = "Uwagi mogą mieć maksymalnie 500 znaków.")]
        [Display(Name = "Powód korekty / uwagi")]
        public string? Notes { get; set; }
    }
}
