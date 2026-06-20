using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class IssueViewModel
    {
        [Required(ErrorMessage = "Magazyn jest wymagany.")]
        [Display(Name = "Magazyn")]
        public int WarehouseId { get; set; }

        [Required(ErrorMessage = "Produkt jest wymagany.")]
        [Display(Name = "Produkt")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od zera.")]
        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [MaxLength(100, ErrorMessage = "Numer dokumentu może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Numer dokumentu")]
        public string? SourceDocument { get; set; }

        [MaxLength(500, ErrorMessage = "Uwagi mogą mieć maksymalnie 500 znaków.")]
        [Display(Name = "Powód / uwagi")]
        public string? Notes { get; set; }
    }
}
