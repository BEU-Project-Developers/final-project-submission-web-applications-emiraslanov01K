using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaterManagementSystem.Models
{
    public class TeamMember
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Komanda üzvünün adı tələb olunur.")]
        [StringLength(100)]
        public string Name { get; set; } // Məs: "Henry", "Jemes Born"

        [StringLength(255)]
        public string? ImagePath { get; set; } // Məs: "img/team-1.jpg"

        // Foreign Key üçün
        [Required(ErrorMessage = "Peşə seçilməlidir.")]
        public int ProfessionId { get; set; }

        // Naviqasiya propertisi: Komanda üzvünün peşəsi
        [ForeignKey("ProfessionId")]
        public virtual Profession? Profession { get; set; }
    }
}