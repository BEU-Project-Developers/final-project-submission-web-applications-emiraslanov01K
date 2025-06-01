using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaterManagementSystem.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tədbir başlığı tələb olunur.")]
        [StringLength(100)]
        public string Title { get; set; } // Məs: "Wedding", "Corporate"

        
        [StringLength(255)]
        public string ImagePath { get; set; } // Məs: "img/event-1.jpg"
        [NotMapped]
        public IFormFile Photo { get; set; }
    }
}