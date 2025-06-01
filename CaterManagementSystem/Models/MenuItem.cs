using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaterManagementSystem.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yemək adı tələb olunur.")]
        [StringLength(100)]
        public string Name { get; set; } // Məs: "Paneer Tikka"

        [Required(ErrorMessage = "Açıqlama tələb olunur.")]
        [StringLength(500)]
        public string Description { get; set; } // Məs: "Marinated cottage cheese..."

        [Required(ErrorMessage = "Qiymət tələb olunur.")]
        [Column(TypeName = "decimal(18,2)")] // Valyuta üçün uyğun tip
        public decimal Price { get; set; } // Məs: 12.00

        [StringLength(255)]
        public string? ImagePath { get; set; } // Məs: "img/menu/paneer-tikka.jpg"
    }
}