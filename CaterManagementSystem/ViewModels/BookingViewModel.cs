// ViewModels/BookingViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; } // Redaktə zamanı istifadə olunacaq

        [Required(ErrorMessage = "Ölkə seçimi tələb olunur.")]
        [Display(Name = "Ölkə")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Şəhər seçimi tələb olunur.")]
        [Display(Name = "Şəhər")]
        public string City { get; set; }

        [Required(ErrorMessage = "Məkan seçimi tələb olunur.")]
        [Display(Name = "Məkan")]
        public string Place { get; set; }

        [Display(Name = "Rezerv İdentifikatoru (əgər varsa)")]
        public string? BookingIdentifier { get; set; }

        [Required(ErrorMessage = "Saray sayı aralığı seçimi tələb olunur.")]
        [Display(Name = "Saray Sayı Aralığı")]
        public string NumberOfPalacesRange { get; set; }

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
        // Tarixin gələcəkdə olması üçün validasiya əlavə edilə bilər
        public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1); // Default olaraq sabah

        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        [Display(Name = "Əlaqə E-poçtu")]
        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Əlavə Qeydlər")]
        public string? Notes { get; set; }

        // Select list-lər üçün məlumatlar (Controller-dən doldurulacaq)
        // public SelectList Countries { get; set; }
        // public SelectList Cities { get; set; }
        // ... digər select listlər ...
    }
}