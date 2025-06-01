using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models // Namespace əlavə etdim
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")] // ErrorMessage əlavə etdim
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin.")] // ErrorMessage əlavə etdim
        [StringLength(50)]
        public string? Phone { get; set; }

        // Form üçün başlıq və mətnləri də burada saxlaya bilərsiniz (əvvəlki kimi)
        // [StringLength(200)]
        // public string? FormIntroText { get; set; }
    }
}