namespace CaterManagementSystem.Areas.Admin.ViewModels // və ya CaterManagementSystem.ViewModels
{
    public class DashboardStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveServicesCount { get; set; } // Service modeliniz varsa
        public int TotalChefs { get; set; }          // TeamMember (Chef) modeliniz varsa
        public int TotalEvents { get; set; }         // Event modeliniz varsa
        
    }
}