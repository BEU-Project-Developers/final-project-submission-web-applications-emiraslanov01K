// Models/MenuItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaterManagementSystem.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Menyu elementinin adı tələb olunur.")]
        [StringLength(150)]
        public string Name { get; set; } // Məs: "Paneer Tikka"

        [StringLength(500)]
        public string? Description { get; set; } // Məs: "Marinated cottage cheese grilled..."

        [Required(ErrorMessage = "Qiymət tələb olunur.")]
        [Column(TypeName = "decimal(18,2)")] // Qiymət üçün uyğun tip
        public decimal Price { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; } // Hər menyu elementi üçün şəkil

        [StringLength(100)]
        public string? Category { get; set; } // Məs: "Appetizers", "Main Course", "Desserts", "Chef's Specialties"

        public bool IsSpecialty { get; set; } = false; // "Chef's Specialties" kimi qeyd etmək üçün
        public bool IsVegetarian { get; set; } = false;
        public bool IsGlutenFree { get; set; } = false;
        // Digər allergen və ya pəhriz məlumatları

        public int DisplayOrder { get; set; }
        public bool IsAvailable { get; set; } = true; // Mövcud olub-olmaması
    }
}