// Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace CaterManagementSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı tələb olunur.")]
        [StringLength(100, ErrorMessage = "İstifadəçi adı 3-100 simvol aralığında olmalıdır.", MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tam ad tələb olunur.")]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public string? ConfirmationToken { get; set; }
        public DateTime? ConfirmationTokenExpiryDate { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryDate { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        // Models/User.cs
        // ...
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiryDate { get; set; }
        // ...

        // Naviqasiya Propertiləri
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual UserDetails? UserDetails { get; set; } // Birə-bir əlaqə
    }
}