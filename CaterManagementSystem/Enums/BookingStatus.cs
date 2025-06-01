
using System.ComponentModel.DataAnnotations; // Display atributu üçün

namespace CaterManagementSystem.Models.Enums  // booking üçün status enum classı 
{
    public enum BookingStatus
    {
        [Display(Name = "Gözləmədə")]
        Pending,

        [Display(Name = "Təsdiqlənib")]
        Confirmed,

        [Display(Name = "Ləğv Edilib")]
        Cancelled,

        [Display(Name = "Tamamlanıb")]
        Completed
    }
}