using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class SupplierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa dostawcy jest wymagana.")]
        [MaxLength(200, ErrorMessage = "Nazwa może mieć maksymalnie 200 znaków.")]
        [Display(Name = "Nazwa dostawcy")]
        public string Name { get; set; } = string.Empty;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "NIP musi składać się z dokładnie 10 cyfr.")]
        [Display(Name = "NIP")]
        public string? NIP { get; set; }

        [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
        [MaxLength(200, ErrorMessage = "Email może mieć maksymalnie 200 znaków.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [MaxLength(30, ErrorMessage = "Telefon może mieć maksymalnie 30 znaków.")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [MaxLength(200)]
        [Display(Name = "Ulica")]
        public string? Street { get; set; }

        [MaxLength(20)]
        [Display(Name = "Numer budynku")]
        public string? BuildingNumber { get; set; }

        [MaxLength(20)]
        [Display(Name = "Kod pocztowy")]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        [Display(Name = "Miasto")]
        public string? City { get; set; }

        [MaxLength(100)]
        [Display(Name = "Kraj")]
        public string? Country { get; set; }

        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;
    }
}
