// Models/ServiceOffering.cs
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Xidmət adı tələb olunur.")]
        [StringLength(100)]
        public string Title { get; set; } // Məs: "Wedding Services"

        [Required(ErrorMessage = "Xidmət açıqlaması tələb olunur.")]
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; } // Hər xidmət üçün ayrıca bir şəkil ola bilər (şəkildə görünmür)

        [StringLength(50)]
        public string ButtonText { get; set; } = "Read More"; // Default dəyər

        [StringLength(255)]
        public string? ButtonUrl { get; set; } // "Read More" düyməsinin yönləndirəcəyi URL

        public int DisplayOrder { get; set; } // Saytda göstərilmə sırası
        public bool IsActive { get; set; } = true; // Aktiv və ya passiv olması
    }
}