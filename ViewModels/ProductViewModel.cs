using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana.")]
        [MaxLength(200, ErrorMessage = "Nazwa może mieć maksymalnie 200 znaków.")]
        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Kod katalogowy jest wymagany.")]
        [MaxLength(50, ErrorMessage = "Kod katalogowy może mieć maksymalnie 50 znaków.")]
        [Display(Name = "Kod katalogowy")]
        public string CatalogNumber { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Minimalny stan jest wymagany.")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimalny stan nie może być ujemny.")]
        [Display(Name = "Minimalny stan")]
        public int MinimumStock { get; set; }

        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;
    }
}