// ViewModels/EnterResetCodeViewModel.cs
using System.ComponentModel.DataAnnotations;
namespace CaterManagementSystem.ViewModels
{
    public class EnterResetCodeViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Hidden input ilə ötürüləcək

        [Required(ErrorMessage = "Təsdiq kodu tələb olunur.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 rəqəmli olmalıdır.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Kod yalnız rəqəmlərdən ibarət olmalıdır.")]
        [Display(Name = "Təsdiq Kodu")]
        public string Code { get; set; }
    }
}