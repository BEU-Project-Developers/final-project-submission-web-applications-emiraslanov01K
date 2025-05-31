// Models/AboutUsSection.cs
using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models
{
    public class About
    {
        [Key]
        public int Id { get; set; } // Adətən yalnız bir qeyd olacaq, Id=1

        [Required(ErrorMessage = "Başlıq etiketi tələb olunur.")]
        [StringLength(100)]
        public string TitleTag { get; set; } // Məs: "ABOUT US"

        [Required(ErrorMessage = "Əsas başlıq tələb olunur.")]
        [StringLength(200)]
        public string MainTitle { get; set; } // Məs: "Trusted By 200+ satisfied clients"

        [Required(ErrorMessage = "Açıqlama mətni tələb olunur.")]
        [StringLength(1000)]
        public string Description { get; set; } // Uzun mətn

        [StringLength(255)]
        public string? ImagePath { get; set; } // Bölmə üçün əsas şəkil (sol tərəfdəki böyük şəkil)

        // Kiçik xüsusiyyət mətnləri (bunları ayrı bir cədvələ də çıxarmaq olar, əgər çoxdursa)
        [StringLength(100)]
        public string? Feature1Text { get; set; } // Məs: "Fresh and Fast food Delivery"
        [StringLength(100)]
        public string? Feature2Text { get; set; } // Məs: "24/7 Customer Support"
        [StringLength(100)]
        public string? Feature3Text { get; set; } // Məs: "Easy Customization Options"
        [StringLength(100)]
        public string? Feature4Text { get; set; } // Məs: "Delicious Deals for Delicious Meals"

        [StringLength(100)]
        public string? ButtonText { get; set; } // Məs: "About Us"
        [StringLength(255)]
        public string? ButtonUrl { get; set; } // Düymənin yönləndirəcəyi URL
    }
}