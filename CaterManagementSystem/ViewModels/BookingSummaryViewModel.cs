using CaterManagementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
namespace CaterManagementSystem.ViewModels
{
    public class BookingSummaryViewModel // Tək bir rezerv üçün qısa məlumat
    {
        public int Id { get; set; }

        [Display(Name = "Rezerv Kodu")]
        public string BookingIdentifier { get; set; }

        [Display(Name = "İstifadəçi Adı")] // YENİ ƏLAVƏ EDİLDİ
        public string UserName { get; set; }

        [Display(Name = "İstifadəçi E-poçtu")]
        public string UserEmail { get; set; }

        [Display(Name = "Xidmət Növü")]
        public string ServiceType { get; set; }

        [Display(Name = "Rezerv Tarixi")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Display(Name = "Məkan")]
        public string Place { get; set; }

        [Display(Name = "Şəhər")]
        public string City { get; set; }

        [Display(Name = "Status")]
        public BookingStatus Status { get; set; }

        [Display(Name = "Yaradılma Tarixi")] // YENİ ƏLAVƏ EDİLDİ (Admin üçün faydalı ola bilər)
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}