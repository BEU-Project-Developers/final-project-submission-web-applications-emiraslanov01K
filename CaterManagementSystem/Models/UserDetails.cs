// SchoolSystem.Models/UserDetails.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CaterManagementSystem.Models;
using Microsoft.AspNetCore.Http; // IFormFile üçün

namespace SchoolSystem.Models
{
    public class UserDetails
    {
        [Key]
        public int Id { get; set; }



        [Required(ErrorMessage = "İstifadəçi adı mütləqdir.")]
        [MaxLength(50)]
        public string Username { get; set; } // User.Username ilə eyni olmalıdır və bəlkə də unikal

        [Required(ErrorMessage = "Tam ad mütləqdir.")]
        [MaxLength(100)]
        public string Fullname { get; set; } // User.Fullname ilə eyni olmalıdır

        [MaxLength(255)]
        public string? ImagePath { get; set; } // Profil şəkli üçün yol (nullable ola bilər)

        [NotMapped] // Bu, bazaya yazılmayacaq, yalnız fayl yükləmək üçün istifadə olunacaq
        public IFormFile? Photo { get; set; }

        // User ilə bir-birə əlaqə (One-to-One relationship)
        public int UserId { get; set; } // Xarici açar (Foreign Key) User cədvəlinə
        public User User { get; set; } = null!; // Naviqasiya xüsusiyyəti
    }
}