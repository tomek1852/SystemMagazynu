using System.ComponentModel.DataAnnotations;

namespace SystemMagazynu.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa kategorii jest wymagana.")]
        [MaxLength(100, ErrorMessage = "Nazwa może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Nazwa kategorii")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Opis może mieć maksymalnie 500 znaków.")]
        [Display(Name = "Opis")]
        public string? Description { get; set; }
    }
}
