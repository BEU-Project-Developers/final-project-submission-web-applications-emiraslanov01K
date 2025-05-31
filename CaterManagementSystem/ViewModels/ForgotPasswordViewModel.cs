using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        [Display(Name = "E-poçt Ünvanınız")]
        public string Email { get; set; }
    }
}