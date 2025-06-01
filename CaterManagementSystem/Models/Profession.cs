using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // List<TeamMember> üçün

namespace CaterManagementSystem.Models
{
    public class Profession
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Peşə adı tələb olunur.")]
        [StringLength(100)]
        public string Name { get; set; } // Məs: "Decoration Chef", "Executive Chef"

        // Naviqasiya propertisi: Bu peşəyə sahib olan komanda üzvləri
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}