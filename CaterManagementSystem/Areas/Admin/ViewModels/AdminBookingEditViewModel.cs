// Areas/Admin/Controllers/BookingsController.cs faylının sonunda və ya Areas/Admin/ViewModels/AdminBookingEditViewModel.cs
using CaterManagementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System; // DateTime üçün

//Əgər ayrıca fayldadırsa, namespace-i düzgün təyin edin:
 namespace CaterManagementSystem.Areas.Admin.ViewModels
{
    public class AdminBookingEditViewModel
    {
        public int Id { get; set; }
        public string? BookingIdentifier { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? ServiceType { get; set; }
        public DateTime BookingDate { get; set; }

        [Display(Name = "Hazırkı Status")]
        public BookingStatus CurrentStatus { get; set; }

        [Required(ErrorMessage = "Yeni status seçilməlidir.")]
        [Display(Name = "Yeni Status")]
        public BookingStatus NewStatus { get; set; } // Artıq enum tipindədir

        [Phone(ErrorMessage = "Düzgün telefon nömrəsi daxil edin.")]
        [Display(Name = "Əlaqə Nömrəsi")]
        public string? ContactNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Admin Qeydləri")]
        public string? Notes { get; set; }
    }
}



