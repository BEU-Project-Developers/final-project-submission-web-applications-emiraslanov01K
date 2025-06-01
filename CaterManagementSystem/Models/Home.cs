using System.ComponentModel.DataAnnotations;

namespace CaterManagementSystem.Models
{
    public class Home
    {
        [Key]
        public int Id { get; set; } 

        [Required(ErrorMessage = "Başlıq etiketi tələb olunur.")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Əsas başlıq tələb olunur.")]
        [StringLength(200)]
        public string Subtitle { get; set; }
    }
}
