// Models/UserDetails.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // IFormFile üçün

namespace CaterManagementSystem.Models // User ilə eyni namespace-də olduğunu güman edirəm
{
    public class UserDetails
    {
        [Key]
        // UserId xarici açar olacaq və eyni zamanda UserDetails üçün əsas açar olacaq
        // Bu, birə-bir əlaqəni daha güclü edir.
        // Alternativ olaraq, ayrıca bir Id və UserId (FK) ola bilər,
        // amma əsas açar paylaşımı (shared primary key) one-to-one üçün daha təmizdir.
        // Ancaq sizin controller kodunuz UserId-nin FK olmasına daha çox uyğundur.
        // Hər iki variantı da aşağıda göstərirəm, birini seçin.

        // Variant 1: UserDetails üçün ayrıca Id, UserId isə FK (Sizin Controller-ə daha uyğun)
        public int Id { get; set; } // UserDetails-in öz müstəqil Id-si

        [MaxLength(255)]
        public string? ImagePath { get; set; } // Profil şəkli üçün yol (nullable ola bilər, default təyin ediləcək)

        [NotMapped] // Bu, bazaya yazılmayacaq, yalnız fayl yükləmək üçün istifadə olunacaq
        public IFormFile? Photo { get; set; }

        // User ilə bir-birə əlaqə (One-to-One relationship)
        [ForeignKey("User")] // Bu atribut User naviqasiya propertisinin FK-nı təyin edir
        public int UserId { get; set; } // Xarici açar (Foreign Key) User cədvəlinə
        public virtual User User { get; set; } = null!; // Naviqasiya xüsusiyyəti


        // Variant 2: UserId həm PK, həm FK (Shared Primary Key Association)
        /*
        [Key, ForeignKey("User")] // UserId həm PK, həm də User-ə FK
        public int UserId { get; set; }

        [MaxLength(255)]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }

        public virtual User User { get; set; } = null!;
        */
    }
}