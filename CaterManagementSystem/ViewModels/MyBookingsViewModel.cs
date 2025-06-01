using CaterManagementSystem.ViewModels;

namespace CaterManagementSystem.ViewModels
{

    public class MyBookingsViewModel // Bu, istifadəçinin öz rezervlərini göstərmək üçün istifadə oluna bilər
    {
        public List<BookingSummaryViewModel> Bookings { get; set; }
        public MyBookingsViewModel()
        {
            Bookings = new List<BookingSummaryViewModel>();
        }
        // Səhifələmə və ya filter məlumatları da bura əlavə edilə bilər
    }
}