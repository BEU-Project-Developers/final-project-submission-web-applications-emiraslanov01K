// Models/GalleryImage.cs
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models
{
    public class GalleryImage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Şəkil yolu tələb olunur.")]
        [StringLength(255)]
        public string ImagePath { get; set; }

        [StringLength(100)]
        public string? Title { get; set; } // Şəkil üzərindəki mətn (Məs: "Wedding")

        [StringLength(200)]
        public string? AltText { get; set; } // Şəkil üçün alternativ mətn (SEO və əlçatanlıq üçün)

        [StringLength(50)]
        public string? Category { get; set; } // Şəkilləri kateqoriyalara bölmək üçün (Məs: "Wedding", "Corporate")

        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; } = false; // Əsas səhifədə və ya xüsusi yerdə göstərmək üçün
    }
}