// Models/TeamMember.cs
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models
{
    public class TeamMember
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Komanda üzvünün adı tələb olunur.")]
        [StringLength(100)]
        public string Name { get; set; } // Məs: "Henry"

        [Required(ErrorMessage = "Vəzifəsi tələb olunur.")]
        [StringLength(100)]
        public string Position { get; set; } // Məs: "Decoration Chef", "Executive Chef"

        [StringLength(255)]
        public string? ImagePath { get; set; } // Komanda üzvünün şəkli

        [StringLength(500)]
        public string? Bio { get; set; } // Qısa tərcümeyi-hal (şəkildə görünmür, amma faydalı ola bilər)

        // Sosial media linkləri (opsional)
        [StringLength(255)]
        public string? FacebookUrl { get; set; }
        [StringLength(255)]
        public string? TwitterUrl { get; set; }
        [StringLength(255)]
        public string? InstagramUrl { get; set; }
        [StringLength(255)]
        public string? LinkedInUrl { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}