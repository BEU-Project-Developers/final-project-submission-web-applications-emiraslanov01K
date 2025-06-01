using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Gerekirse

namespace CaterManagementSystem.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Xidmət adı tələb olunur.")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Xidmət qısa açıqlaması tələb olunur.")]
        [StringLength(500)]
        public string Description { get; set; } // Qısa təsvir (kartlarda görünən)

        [StringLength(255)]
        public string? ImagePath { get; set; } // İkon sinfi (məs: "fas fa-cheese") və ya şəkil yolu

        [StringLength(50)]
        public string ButtonText { get; set; } = "Read More";

        // Navigation property for the detailed description
        public virtual ServiceDescription? ServiceDescription { get; set; }
    }
}