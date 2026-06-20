using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class WarehouseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa magazynu jest wymagana.")]
        [MaxLength(200, ErrorMessage = "Nazwa może mieć maksymalnie 200 znaków.")]
        [Display(Name = "Nazwa magazynu")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ulica jest wymagana.")]
        [MaxLength(200, ErrorMessage = "Ulica może mieć maksymalnie 200 znaków.")]
        [Display(Name = "Ulica")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer budynku jest wymagany.")]
        [MaxLength(20, ErrorMessage = "Numer budynku może mieć maksymalnie 20 znaków.")]
        [Display(Name = "Numer budynku")]
        public string BuildingNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [MaxLength(20, ErrorMessage = "Kod pocztowy może mieć maksymalnie 20 znaków.")]
        [Display(Name = "Kod pocztowy")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Miasto jest wymagane.")]
        [MaxLength(100, ErrorMessage = "Miasto może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Miasto")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kraj jest wymagany.")]
        [MaxLength(100, ErrorMessage = "Kraj może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Kraj")]
        public string Country { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Opis może mieć maksymalnie 500 znaków.")]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;
    }
}
