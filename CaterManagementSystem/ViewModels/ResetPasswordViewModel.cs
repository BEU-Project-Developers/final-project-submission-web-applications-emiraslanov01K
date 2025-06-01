// ViewModels/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;
namespace CaterManagementSystem.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Hidden input

        [Required(ErrorMessage = "Təsdiq kodu tələb olunur.")] // Əgər bu View-a birbaşa gəlinirsə
        public string Code { get; set; } // Hidden input (EnterResetCode-dan sonra)

        [Required(ErrorMessage = "Yeni şifrə tələb olunur.")]
        [StringLength(100, ErrorMessage = "{0} ən azı {2} simvol uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrə")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrəni Təsdiqləyin")]
        [Compare("Password", ErrorMessage = "Şifrə və təsdiq şifrəsi uyğun gəlmir.")]
        public string ConfirmPassword { get; set; }
    }
}