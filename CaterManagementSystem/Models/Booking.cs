// Models/Booking.cs (Nümunə)
using CaterManagementSystem.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaterManagementSystem.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        // İstifadəçi ilə əlaqə
        public int UserId { get; set; } // Bu rezervi edən istifadəçinin ID-si
        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Naviqasiya propertisi

        [Required(ErrorMessage = "Ölkə seçimi tələb olunur.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Şəhər seçimi tələb olunur.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Məkan seçimi tələb olunur.")]
        public string Place { get; set; }

        // Sizin Booking.cshtml-də "Book Identifier" var, bu nə üçündür?
        // Əgər hər rezervin unikal bir kodu olacaqsa, onu burada saxlaya bilərsiniz.
        [Display(Name = "Rezerv İdentifikatoru")]
        public string? BookingIdentifier { get; set; } // Bu, bəlkə də istifadəçinin gördüyü koddur

        [Required(ErrorMessage = "Saray sayı aralığı seçimi tələb olunur.")]
        [Display(Name = "Saray Sayı Aralığı")]
        public string NumberOfPalacesRange { get; set; } // Məsələn, "100-200"

        [Required(ErrorMessage = "Xidmət növü seçimi tələb olunur.")]
        [Display(Name = "Xidmət Növü")]
        public string ServiceType { get; set; }

        [Required(ErrorMessage = "Əlaqə nömrəsi tələb olunur.")]
        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin.")]
        [Display(Name = "Əlaqə Nömrəsi")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Tarix seçimi tələb olunur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Tarix")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        public string Email { get; set; } // Bu, istifadəçinin e-poçtu ilə eyni olmaya da bilər (əgər fərqli bir əlaqə e-poçtudursa)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [Display(Name = "Status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Əlavə qeydlər və ya detallar
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }
    }
}