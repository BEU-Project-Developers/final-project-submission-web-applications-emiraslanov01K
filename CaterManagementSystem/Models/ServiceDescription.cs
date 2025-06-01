using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaterManagementSystem.Models
{
    public class ServiceDescription
    {
        [Key]
        public int Id { get; set; } // ServiceDescription-ın öz Primary Key-i

        [Required(ErrorMessage = "Başlıq tələb olunur.")] // Bu başlıq detallar səhifəsi üçün spesifik ola bilər
        [StringLength(100)]
        public string Title { get; set; }

        // Bu sahələr sizin təqdim etdiyiniz modeldəndir
        public int GuestCount { get; set; }
        public int PerPersonPay { get; set; }
        public string DateWithMonths { get; set; } // Məs: "3-6 ay planlama"

        // Service modelinə Foreign Key
        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; } // Navigation property
    }
}