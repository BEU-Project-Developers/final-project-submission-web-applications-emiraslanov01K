using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.ViewModels // Namespace-i layihənizə uyğunlaşdırın
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-poçt və ya istifadəçi adı tələb olunur.")]
        [Display(Name = "E-poçt və ya İstifadəçi adı")]
        public string EmailOrUserName { get; set; } // İstifadəçi həm e-poçt, həm də istifadəçi adı ilə daxil ola bilər

        [Required(ErrorMessage = "Şifrə tələb olunur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [Display(Name = "Məni xatırla?")]
        public bool RememberMe { get; set; }
    }
}