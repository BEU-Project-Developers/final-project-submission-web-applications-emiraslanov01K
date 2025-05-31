// Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // NotMapped üçün
using Microsoft.AspNetCore.Http;
using SchoolSystem.Models; // IFormFile üçün (əgər profil şəkli User səviyyəsindədirsə)

namespace CaterManagementSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı tələb olunur.")]
        [StringLength(100, ErrorMessage = "İstifadəçi adı 3-100 simvol aralığında olmalıdır.", MinimumLength = 3)]
        public string UserName { get; set; } // SchoolSystem-dəki Username ilə eyni

        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        [StringLength(256)]
        public string Email { get; set; }

        // Fullname UserDetails-ə köçürülə bilər və ya burada qala bilər.
        // Sadəlik üçün burada saxlayaq.
        [Required(ErrorMessage = "Tam ad tələb olunur.")]
        [StringLength(150)]
        public string FullName { get; set; } // SchoolSystem-dəki Fullname ilə eyni

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public string? ConfirmationToken { get; set; } // EmailConfirmationToken
        public DateTime? ConfirmationTokenExpiryDate { get; set; } // EmailConfirmationTokenExpiry

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryDate { get; set; } // PasswordResetTokenExpiry

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Naviqasiya Propertiləri
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public UserDetails? UserDetails { get; set; } // Birə-bir əlaqə UserDetails ilə

        // Əgər User səviyyəsində də default bir şəkil və ya yükləmə imkanı olacaqsa:
        [StringLength(255)]
        public string? ImagePath { get; set; } // Default və ya UserDetails-dəki ilə sinxronlaşdırıla bilər

        [NotMapped] // Bu, verilənlər bazasına düşməyəcək, yalnız fayl yükləmək üçündür
        public IFormFile? Photo { get; set; }
    }
}