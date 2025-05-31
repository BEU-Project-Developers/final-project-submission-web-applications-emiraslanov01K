// ViewModels/EditProfileViewModel.cs
using Microsoft.AspNetCore.Http; // IFormFile üçün
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.ViewModels
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; } // Hidden input ilə ötürüləcək

        [Required(ErrorMessage = "İstifadəçi adı tələb olunur.")]
        [StringLength(100, ErrorMessage = "İstifadəçi adı 3-100 simvol aralığında olmalıdır.", MinimumLength = 3)]
        [Display(Name = "İstifadəçi adı")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        [Display(Name = "E-poçt Ünvanı")]
        public string Email { get; set; } // E-poçt redaktəsi üçün xüsusi təsdiqləmə lazım ola bilər

        [Required(ErrorMessage = "Tam ad tələb olunur.")]
        [StringLength(150, ErrorMessage = "Tam ad maksimum 150 simvol ola bilər.")]
        [Display(Name = "Tam Adınız")]
        public string FullName { get; set; }

        [Display(Name = "Profil Şəkli")]
        public IFormFile? Photo { get; set; } // Yeni şəkil yükləmək üçün

        public string? CurrentImagePath { get; set; } // Mövcud şəkli göstərmək üçün (View-da istifadə olunur)

        // Şifrə Dəyişikliyi Sahələri (Opsional)
        [DataType(DataType.Password)]
        [Display(Name = "Hazırkı Şifrə")]
        public string? CurrentPassword { get; set; } // Yeni şifrə təyin ediləcəksə tələb olunur

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} ən azı {2} simvol uzunluğunda olmalıdır.", MinimumLength = 6)]
        [Display(Name = "Yeni Şifrə")]
        public string? NewPassword { get; set; } // Yalnız dəyişdirmək istədikdə doldurulur

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifrəni Təsdiqləyin")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifrə və təsdiq şifrəsi uyğun gəlmir.")]
        public string? ConfirmNewPassword { get; set; }
    }
}