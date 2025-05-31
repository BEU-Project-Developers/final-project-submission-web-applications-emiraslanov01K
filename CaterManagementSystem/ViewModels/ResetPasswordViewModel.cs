using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Bu, adətən hidden input ilə ötürülür

        [Required(ErrorMessage = "Yeni şifrə tələb olunur.")]
        [StringLength(100, ErrorMessage = "{0} ən azı {2} və ən çox {1} simvol uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrə")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrəni Təsdiqləyin")]
        [Compare("Password", ErrorMessage = "Şifrə və təsdiq şifrəsi uyğun gəlmir.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Token { get; set; } // Bu, adətən hidden input ilə ötürülür
    }
}