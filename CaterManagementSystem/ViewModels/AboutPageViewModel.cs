using CaterManagementSystem.Models; // About, User (və ya Customer), TeamMember, Event modelləri üçün

namespace CaterManagementSystem.ViewModels
{
    public class AboutPageViewModel
    {
        public About? AboutSectionData { get; set; } 

        // Statistik məlumatlar
        public int HappyCustomersCount { get; set; }
        public int ExpertChefsCount { get; set; }
        public int EventsCompleteCount { get; set; }

        public string VideoUrl { get; set; }
    }
}